#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Serialization;
using HttpRequest = Microsoft.AspNetCore.Http.HttpRequest;

namespace Nexus.Link.Libraries.Web.AspNet.Serialization
{
    public static class SerializationExtensions
    {

        /// <summary>
        /// Serialize the <paramref name="source"/>.
        /// </summary>
        public static async Task<RequestData> FromAsync(this RequestData target, HttpRequest source, CancellationToken cancellationToken = default)
        {
            // Verify that we can convert the string to an HttpMethod
            VerifyMethodName(source.Method);
            target.Method = source.Method;

            target.EncodedUrl = source.GetEncodedUrl();
            target.Headers = new Dictionary<string, StringValues>();
            target.ContentType = source.ContentType;
            target.ContentLength = source.ContentLength;
            foreach (var requestHeader in source.Headers)
            {
                target.Headers[requestHeader.Key] = requestHeader.Value;
            }


            target.BodyAsString = await source.GetRequestBodyAsync();

            return target;

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

        /// <summary>
        /// Serialize to <see cref="HttpResponseMessage"/>.
        /// </summary>
        public static async Task<HttpRequestCreate> FromAsync(this HttpRequestCreate target, HttpRequest source, double priority, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var requestData = await new RequestData().FromAsync(source, cancellationToken);
            target.Method = requestData.Method;
            target.Url = requestData.EncodedUrl;
            target.Metadata.Priority = priority;
            target.Headers = RemoveContentHeaders(source.Headers);
            foreach (var () in source.Headers)
            {
                
            }
            target.Content = requestData.BodyAsString;
            return target;
        }

        private static Dictionary<string, StringValues> RemoveContentHeaders(IHeaderDictionary sourceHeaders)
        {
            return sourceHeaders.Where(h => !h.Key.StartsWith("Content-")).ToDictionary(v => v.Key, v => v.Value);
        }

        // https://www.programmersought.com/article/49936898629/
        public static async Task<string> GetRequestBodyAsync(this HttpRequest request)
        {
            FulcrumAssert.IsNotNull(request.Body, CodeLocation.AsString());
            request.EnableBuffering();
            request.Body.Position = 0;
            var sr = new StreamReader(request.Body);
            var body = await sr.ReadToEndAsync();
            request.Body.Position = 0;
            if (body == "") FulcrumAssert.IsTrue(request.ContentLength == null || request.ContentLength.Value == 0, CodeLocation.AsString());
            return body;
        }
    }
}
#endif