using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;

namespace Nexus.Link.Libraries.Web.Serialization
{
    /// <summary>
    /// Serialization of an <see cref="HttpRequestMessage"/>.
    /// </summary>
    public class RequestData : IValidatable
    {
        /// <summary>
        /// A unique Id for this request
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// The <see cref="HttpMethod"/> name.
        /// </summary>
        public string Method { get; set; } = "Get";

        /// <summary>
        /// The encoded URL
        /// </summary>
        public string EncodedUrl { get; set; } = "";

        /// <summary>
        /// The request headers.
        /// </summary>
        public Dictionary<string, StringValues> Headers { get; set; } = new Dictionary<string, StringValues>();

        /// <summary>
        /// The request body, serialized to a string.
        /// </summary>
        public string BodyAsString { get; set; }

        /// <summary>
        /// The content type of the request body.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content length of the request body
        /// </summary>
        public long? ContentLength { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Method} {EncodedUrl}";

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Method, nameof(Method), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(EncodedUrl, nameof(EncodedUrl), errorLocation);
            FulcrumValidate.IsTrue(IsValidUri(EncodedUrl), errorLocation, $"{propertyPath}.{nameof(EncodedUrl)} is not a valid URL.");
            FulcrumValidate.IsTrue(HttpMethodExists(), errorLocation, $"{Method} is not a valid HttpMethod");
            if (BodyAsString != null)
            {
                FulcrumValidate.IsTrue(JsonHelper.TryDeserializeObject(BodyAsString, out JToken _), errorLocation,
                    $"{propertyPath}.{nameof(BodyAsString)} must be JSON.");
            }
            FulcrumValidate.IsTrue(ValidHeaders(), errorLocation, "One or more headers are invalid.");

            bool HttpMethodExists()
            {
                var propertyInfo = typeof(System.Net.Http.HttpMethod).GetProperty(Method,
                    BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public);
                return propertyInfo != null;
            }

            bool IsValidUri(string url)
            {
                var result = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                             && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                return result;
            }

            bool ValidHeaders()
            {
                if (Headers != null && Headers.Any())
                {
                    foreach (var header in Headers)
                    {
                        if (string.IsNullOrWhiteSpace(header.Key) || header.Value.ToString() == null || !header.Value.Any())
                            return false;

                        if (header.Value.Any(string.IsNullOrWhiteSpace))
                        {
                            return false;
                        }

                        if (header.Key.StartsWith("Content-")) return false;
                    }
                }

                return true;
            }
        }
    }
}
