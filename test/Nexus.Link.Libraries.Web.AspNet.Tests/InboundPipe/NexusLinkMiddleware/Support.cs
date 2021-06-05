#if NETCOREAPP
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware
{
    internal static class Support
    {
        public static void SetRequest(DefaultHttpContext context, string url)
        {
            var request = new DefaultHttpRequest(context);
            var match = Regex.Match(url, "^(https?)://([^/]+)(/[^?]+)(\\?.*)?$");
            request.Scheme = match.Groups[1].Value;
            request.Host = new HostString(match.Groups[2].Value);
            request.PathBase = new PathString("/");
            request.Path = new PathString(match.Groups[3].Value);
            request.Method = "GET";
            request.Body = new MemoryStream();
            request.QueryString = new QueryString(match.Groups[4].Value);
        }
    }
}
#endif
