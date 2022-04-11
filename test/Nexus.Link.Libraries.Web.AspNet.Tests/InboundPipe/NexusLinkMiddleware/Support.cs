#if NETCOREAPP
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Nexus.Link.Libraries.Web.Pipe;
using Newtonsoft.Json.Linq;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware
{
    internal static class Support
    {
        public static HttpRequest SetRequest(this DefaultHttpContext context, string url = "http://example.com/Person/123?test=1", string method = "GET", JObject jObject = null)
        {
            var match = Regex.Match(url, "^(https?)://([^/]+)(/[^?]*)?(\\?.*)?$");
            if (!match.Success)
            {
                throw new ApplicationException($"The url was formatted unexpectedly: {url}");
            }
            var memoryStream = jObject == null ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(jObject.ToString()));

            var request = new DefaultHttpRequest(context)
            {
                Scheme = match.Groups[1].Value,
                Host = new HostString(match.Groups[2].Value),
                PathBase = new PathString("/"),
                Path = new PathString(match.Groups[3].Value),
                Method = method,
                Body = memoryStream,
                ContentLength = memoryStream.Length,
                QueryString = new QueryString(match.Groups[4].Value)
            };
            return request;
        }

        public static HttpRequest CreateRequest(string url = "http://example.com/Person/123?test=1", string method = "GET", JObject jObject = null)
        {
            var context = new DefaultHttpContext();
            return context.SetRequest(url, method, jObject);
        }
        public static HttpRequest SetRequestWithReentryAuthentication(this DefaultHttpContext context, string reentryAuthentication)
        {
            var memoryStream = new MemoryStream();
            var request = new DefaultHttpRequest(context)
            {
                Scheme = "https",
                Host = new HostString("host.example.com"),
                PathBase = new PathString("/"),
                Path = new PathString("/Person/123"),
                Method = "Get",
                Body = memoryStream,
                ContentLength = memoryStream.Length,
                QueryString = new QueryString("?id=23")
            };
            if (reentryAuthentication != null)
            {
                request.Headers.Add(Constants.ReentryAuthenticationHeaderName, reentryAuthentication);
            }
            return request;
        }
    }
}
#endif
