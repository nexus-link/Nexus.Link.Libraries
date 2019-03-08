#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#else
using System.Net.Http;
using System.Threading;

#endif

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    /// <summary>
    /// Compatibility class. Will inherit from different classes; either Microsoft.AspNetCore.Mvc.ControllerBase
    /// for ASP.NET Core Web App or System.Web.Http.ApiController for ASP.NET Web Api.
    /// </summary>
    public class CompabilityInvocationContext
    {
#if NETCOREAPP
        public HttpContext Context { get; }
        public CompabilityInvocationContext(HttpContext context)
        {
            Context = context;
        }
#else
        public HttpRequestMessage RequestMessage { get; }
        public CancellationToken CancellationToken { get; }
        public HttpResponseMessage ResponseMessage { get; set; }
        public CompabilityInvocationContext(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestMessage = request;
            CancellationToken = cancellationToken;
        }
#endif
    }
}