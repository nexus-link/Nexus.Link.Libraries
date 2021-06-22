#if NETCOREAPP
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model
{
    /// <summary>
    /// Serialization of an <see cref="HttpRequest"/>.
    /// </summary>
    public class RequestData
    {
        /// <summary>
        /// A unique Id for this request
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

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
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();

        /// <summary>
        /// The request body, serialized to a string.
        /// </summary>
        public string? BodyAsString { get; set; }

        /// <summary>
        /// The content type of the request body.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content length of the request body
        /// </summary>
        public long? ContentLength { get; set; }

        /// <summary>
        /// Serialize the <paramref name="request"/>.
        /// </summary>
        public async Task<RequestData> FromAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            Method = request.Method;
            // Verify that we can convert the string to an HttpMethod
            VerifyMethodName();

            EncodedUrl = request.GetEncodedUrl();
            Headers = new HeaderDictionary();
            ContentType = request.ContentType;
            ContentLength = request.ContentLength;
            foreach (var requestHeader in request.Headers)
            {
                Headers.Add(requestHeader);
            }
            

            if (request.Body != null && request.Body.CanSeek && request.Body.CanRead)
            {
                request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8);
                BodyAsString = await reader.ReadToEndAsync();
            }

            return this;

            void VerifyMethodName()
            {
                try
                {
                    _ = new HttpMethod(Method);
                }
                catch (Exception e)
                {
                    InternalContract.Fail($"The following HTTP method is not recognized: {Method}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Deserialize back to a <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage ToHttpRequestMessage()
        {
            var httpMethod = new HttpMethod(Method);
            var requestMessage = new HttpRequestMessage(httpMethod, EncodedUrl);
            foreach (var (key, value) in Headers)
            {
                if (key.Contains("Content-")) continue;
                requestMessage.Headers.Add(key, value.ToArray());
            }

            requestMessage.Content = new StringContent(BodyAsString, System.Text.Encoding.UTF8);
            if (ContentType != null)
            {
                requestMessage.Content.Headers.ContentType =
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse(ContentType);
            }
            return requestMessage;
        }
    }
}
#endif