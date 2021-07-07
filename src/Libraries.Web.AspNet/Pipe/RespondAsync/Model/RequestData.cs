﻿#if NETCOREAPP
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

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
        public string BodyAsString { get; set; }

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
            // Verify that we can convert the string to an HttpMethod
            VerifyMethodName(request.Method);
            Method = request.Method;

            EncodedUrl = request.GetEncodedUrl();
            Headers = new HeaderDictionary();
            ContentType = request.ContentType;
            ContentLength = request.ContentLength;
            foreach (var requestHeader in request.Headers)
            {
                Headers.Add(requestHeader);
            }


            BodyAsString = await GetRequestBodyAsync(request);

            return this;

            void VerifyMethodName(string methodName)
            {
                try
                {
                    _ = new HttpMethod(methodName);
                }
                catch (Exception e)
                {
                    InternalContract.Fail($"The following HTTP method is not recognized: {methodName}: {e.Message}");
                }
            }
        }

        // https://devblogs.microsoft.com/aspnet/re-reading-asp-net-core-request-bodies-with-enablebuffering/
        public async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            FulcrumAssert.IsNotNull(request.Body, CodeLocation.AsString());
            request.EnableBuffering();

            // Leave the body open so the next middleware can read it.
            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            // Reset the request body stream position so the next middleware can read it
            request.Body.Position = 0;
            if (body == "") FulcrumAssert.AreEqual(0, request.ContentLength);

            return body;
        }

        public async Task<string> GetRequestBodyAsync(HttpRequestMessage request)
        {
            await request.Content.LoadIntoBufferAsync();
            return await request.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Serialize the <paramref name="request"/>.
        /// </summary>
        public async Task<RequestData> FromAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            Method = request.Method.ToString();
            EncodedUrl = request.RequestUri.AbsoluteUri;
            Headers = new HeaderDictionary();
            ContentType = request.Content.Headers.ContentType.ToString();
            ContentLength = request.Content.Headers.ContentLength;
            foreach (var (key, value) in request.Headers)
            {
                Headers.Add(key, value.ToArray());
            }

            BodyAsString = await GetRequestBodyAsync(request);

            return this;
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

            requestMessage.Content = BodyAsString == null ? null : new StringContent(BodyAsString, System.Text.Encoding.UTF8);
            if (requestMessage.Content != null && ContentType != null)
            {
                requestMessage.Content.Headers.ContentType =
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse(ContentType);
            }
            return requestMessage;
        }
    }
}
#endif