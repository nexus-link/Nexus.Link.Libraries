using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Nexus.Link.Libraries.Web.Serialization
{
    public static class SerializationExtensions
    {

        /// <summary>
        /// Serialize the <paramref name="source"/>.
        /// </summary>
        public static async Task<RequestData> FromAsync(this RequestData target, HttpRequestMessage source, CancellationToken cancellationToken = default)
        {
            target.Method = source.Method.ToString();
            target.EncodedUrl = source.RequestUri.AbsoluteUri;
            target.Headers = new Dictionary<string, StringValues>();
            target.ContentType = source.Content.Headers.ContentType.ToString();
            target.ContentLength = source.Content.Headers.ContentLength;
            foreach (var header in source.Headers)
            {
                target.Headers[header.Key] = header.Value.ToArray();
            }

            target.BodyAsString = await source.GetRequestBodyAsync();

            return target;
        }

        public static async Task<ResponseData> FromAsync(this ResponseData target, HttpResponseMessage source)
        {
            target.StatusCode = source.StatusCode;
            
            foreach (var header in source.Headers)
            {
                target.Headers[header.Key] = header.Value.ToArray();
            }

            await source.Content.LoadIntoBufferAsync();
            target.BodyAsString = source.Content == null ? null : await source.Content.ReadAsStringAsync();
            target.ContentType = source.Content.Headers.ContentType.ToString();
            target.ContentLength = source.Content.Headers.ContentLength;
            return target;
        }

        public static ResponseData From(this ResponseData target, Exception source)
        {
            target.StatusCode = HttpStatusCode.InternalServerError;
            target.BodyAsString = JsonConvert.SerializeObject($"{source}");
            target.ContentType = "application/json";
            target.ContentLength = target.BodyAsString.Length;
            return target;
        }

        public static async Task<string> GetRequestBodyAsync(this HttpRequestMessage request)
        {
            await request.Content.LoadIntoBufferAsync();
            return await request.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Deserialize to a <see cref="HttpRequestMessage"/>.
        /// </summary>
        public static HttpRequestMessage ToHttpRequestMessage(this RequestData source)
        {
            var httpMethod = new HttpMethod(source.Method);
            var requestMessage = new HttpRequestMessage(httpMethod, source.EncodedUrl);
            foreach (var header in source.Headers)
            {
                if (header.Key.ToLowerInvariant().StartsWith("content-")) continue;
                requestMessage.Headers.Add(header.Key, header.Value.ToArray());
            }

            requestMessage.Content = source.BodyAsString == null ? null : new StringContent(source.BodyAsString, System.Text.Encoding.UTF8);
            if (requestMessage.Content != null && source.ContentType != null)
            {
                requestMessage.Content.Headers.ContentType =
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse(source.ContentType);
            }
            return requestMessage;
        }

        /// <summary>
        /// Deserialize to a <see cref="HttpResponseMessage"/>.
        /// </summary>
        public static HttpResponseMessage ToHttpResponseMessage(this ResponseData source, HttpRequestMessage request)
        {
            var target = request.CreateResponse(source.StatusCode);
            foreach (var header in source.Headers)
            {
                if (header.Key.ToLowerInvariant().StartsWith("content-")) continue;
                target.Headers.Add(header.Key, header.Value.ToArray());
            }

            target.Content = source.BodyAsString == null ? null : new StringContent(source.BodyAsString, System.Text.Encoding.UTF8, source.ContentType);
            return target;
        }
    }
}
