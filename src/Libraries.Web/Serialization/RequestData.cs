using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Primitives;

namespace Nexus.Link.Libraries.Web.Serialization
{
    /// <summary>
    /// Serialization of an <see cref="HttpRequestMessage"/>.
    /// </summary>
    public class RequestData
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
    }
}
