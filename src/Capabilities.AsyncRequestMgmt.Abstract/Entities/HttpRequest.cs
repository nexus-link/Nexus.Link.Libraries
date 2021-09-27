using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities
{
    /// <summary>
    /// Describes a httpRequest that a client wants to be executed asynchronously at a remote server.
    /// </summary>
    public class HttpRequest : HttpRequestCreate, IUniquelyIdentifiable<string>
    {
        /// <summary>
        /// The RequestId for the request.
        /// </summary>
        public string Id { get; set; }

        /// <inheritdoc />
        public override void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Id, nameof(Id), CodeLocation.AsString());
            base.Validate(errorLocation, propertyPath);
        }
    }

    /// <summary>
    /// Describes a httpRequest that a client wants to be executed asynchronously at a remote server.
    /// </summary>
    public class HttpRequestCreate : IValidatable
    {
        /// <summary>
        /// Metadata about a request.
        /// </summary>
        public RequestMetadata Metadata { get; set; } = new RequestMetadata();

        /// <summary>
        /// The URL to send the request to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The name of the request. Not necessarily unique.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The HTTP Method
        /// HTTPS, HTTP etc.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// The HTTP content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The HTTP headers
        /// </summary>
        public Dictionary<string, StringValues> Headers { get; set; }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Method, nameof(Method), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Url, nameof(Url), errorLocation);
            FulcrumValidate.IsTrue(IsValidUri(Url), errorLocation, $"{propertyPath}.{nameof(Url)} is not a valid URL.");
            FulcrumValidate.IsTrue(HttpMethodExists(), errorLocation, $"{Method} is not a valid HttpMethod");
            if (Content != null)
            {
                FulcrumValidate.IsTrue(JsonHelper.TryDeserializeObject(Content, out JToken _), errorLocation,
                    $"{propertyPath}.{nameof(Content)} must be JSON.");
            }
            FulcrumValidate.IsTrue(ValidHeaders(), errorLocation, "One or more headers are invalid.");
            FulcrumValidate.IsNotNull(Metadata, propertyPath, nameof(Metadata), errorLocation);
            FulcrumValidate.IsValidated(Metadata, propertyPath, nameof(Metadata), errorLocation);

            if (Name != null)
            {
                FulcrumValidate.IsNotNullOrWhiteSpace(Name, nameof(Name), errorLocation);
            }
            
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
                    }
                }

                return true;
            }
        }
    }

    /// <summary>
    /// Describes the Metadata about a request.
    /// </summary>
    public class RequestMetadata : IValidatable
    {
        /// <summary>
        /// The Priority for the request.
        /// </summary>
        public double Priority { get; set; }

        /// <summary>
        /// Date and time specifying when to give up on the request
        /// </summary>
        public DateTimeOffset? ExecuteBefore { get; set; }

        /// <summary>
        /// Earliest date and time when we should execute the request
        /// </summary>
        public DateTimeOffset? ExecuteAfter { get; set; }

        /// <summary>
        /// Information about if and where we should send a callback to the client when the final response is available.
        /// </summary>
        public RequestCallback Callback { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Callback != null) FulcrumValidate.IsValidated(Callback, propertyPath, nameof(Callback), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0.0, Priority, nameof(Priority), errorLocation);
            FulcrumValidate.IsLessThanOrEqualTo(1.0, Priority, nameof(Priority), errorLocation);
        }
    }

    /// <summary>
    /// Information about where we should send a callback to the client when the final response is available.
    /// </summary>
    public class RequestCallback : IValidatable
    {
        /// <summary>
        /// The URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The HTTP headers
        /// </summary>
        public Dictionary<string, StringValues> Headers { get; set; }

        /// <summary>
        /// Serialized version of a context that will be supplied with the callback.
        /// </summary>
        public string Context { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsTrue(IsValidUri(Url), errorLocation, $"{propertyPath}.{nameof(Url)} is not a valid URL.");
            FulcrumValidate.IsTrue(ValidHeaders(), errorLocation, "One or more headers are invalid.");

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
                    }
                }

                return true;
            }
        }
    }
}