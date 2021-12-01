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
            ValidateHeaders();

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

            void ValidateHeaders()
            {
                if (Headers != null && Headers.Any())
                {
                    var index = 0;
                    foreach (var header in Headers)
                    {
                        index++;
                        FulcrumValidate.IsNotNullOrWhiteSpace(header.Key, "ignore", errorLocation,
                            $"Header {index} in {propertyPath}.{nameof(Headers)} had an empty key.");
                        FulcrumValidate.IsTrue(!header.Key.StartsWith("Content-"), errorLocation,
                            $"Header {header.Key} is not allowed, as it is a content header.");
                        FulcrumValidate.IsNotNullOrWhiteSpace(header.Value.ToString(), "ignore", errorLocation,
                            $"Header {header.Key} had an empty value.");
                        FulcrumValidate.IsTrue(header.Value.Any(), errorLocation,
                            $"Header {header.Key} had no values.");
                        FulcrumValidate.IsTrue(!header.Value.Any(string.IsNullOrWhiteSpace), errorLocation,
                            $"Header {header.Key} had an empty value.");
                    }
                }
            }
        }
    }
}
