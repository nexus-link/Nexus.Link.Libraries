using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Note! Use SaveNexusTestContext in the IN-pipe.
    /// </summary>
    public class PropagateNexusTestHeader : DelegatingHandler
    {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.TryGetValues(Constants.NexusTestContextHeaderName, out _))
            {
                var headerValue = FulcrumApplication.Context.NexusTestContext;
                if (!string.IsNullOrWhiteSpace(headerValue))
                {
                    request.Headers.Add(Constants.NexusTestContextHeaderName, headerValue);
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }

    }
}