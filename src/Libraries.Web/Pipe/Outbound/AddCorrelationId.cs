using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Logging;

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Adds a Fulcrum CorrelationId header to all outgoing requests.
    /// </summary>
    public class AddCorrelationId : DelegatingHandler
    {
        /// <summary>
        /// Adds a Fulcrum CorrelationId to the requests before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(FulcrumApplication.Context.CorrelationId))
            {
                if (FulcrumApplication.Context.CallingClientName != null)
                {
                    // We should have a gotten a correlation id from the calling client.
                    var logLevel = FulcrumApplication.IsInProductionOrProductionSimulation
                        ? LogSeverityLevel.Verbose
                        : LogSeverityLevel.Warning;
                    Log.LogOnLevel(logLevel,
                        $"We have a calling client ({FulcrumApplication.Context.CallingClientName}), but we are missing a correlation id for an outbound request ({request.ToLogString()}).");
                }
            }
            else
            {
                if (!request.Headers.TryGetValues(Constants.FulcrumCorrelationIdHeaderName, out IEnumerable<string> _))
                {
                    request.Headers.Add(Constants.FulcrumCorrelationIdHeaderName,
                        FulcrumApplication.Context.CorrelationId);
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
