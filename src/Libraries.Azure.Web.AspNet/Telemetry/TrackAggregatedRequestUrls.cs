using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;

#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endif
namespace Nexus.Link.Libraries.Azure.Web.AspNet.Telemetry
{
    /// <summary>
    /// TODO
    /// </summary>
    public class TrackAggregatedRequestUrls : CompatibilityDelegatingHandlerWithCancellationSupport
    {
        private readonly IDictionary<string, Regex> _pathAndQueryMatching;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathAndQueryMatching">TODO</param>
#if NETCOREAPP
        public TrackAggregatedRequestUrls(RequestDelegate next, IDictionary<string, Regex> pathAndQueryMatching) : base(next)
        {
            _pathAndQueryMatching = pathAndQueryMatching;
        }
#else
        public TrackAggregatedRequestUrls(IDictionary<string, Regex> pathAndQueryMatching)
        {
            _pathAndQueryMatching = pathAndQueryMatching;
        }
#endif

        protected override async Task InvokeAsync(CompabilityInvocationContext context, CancellationToken cancellationToken)
        {
            FulcrumAssert.IsNotNull(FulcrumApplication.Setup.TelemetryHandler, null, $"When using this handler, setup the {nameof(FulcrumApplication.Setup.TelemetryHandler)}");
            FulcrumAssert.IsNotNull(_pathAndQueryMatching, null, $"Expected {nameof(_pathAndQueryMatching)}");

#if NETCOREAPP
            var pathAndQuery = context.Context.Request.Path;
            var method = context.Context.Request.Method.ToUpperInvariant();
#else
            var pathAndQuery = context.RequestMessage.RequestUri.PathAndQuery;
            var method = context.RequestMessage.Method.ToString().ToUpperInvariant();
#endif

            foreach (var entry in _pathAndQueryMatching)
            {
                var match = entry.Value.Match(pathAndQuery);
                if (match.Success)
                {
                    FulcrumApplication.Setup.TelemetryHandler.TrackEvent("AggregatedRequests", new Dictionary<string, string> {
                        { "AggregatedOn", entry.Key },
                        { "PathAndQuery", pathAndQuery },
                        { "HttpMethod", method },
                    });
                    break;
                }
            }

            await CallNextDelegateAsync(context, cancellationToken);
        }
    }

#if NETCOREAPP
    public static class TrackAggregatedRequestsExtension
    {
        public static IApplicationBuilder UseTrackAggregatedRequestUrls(this IApplicationBuilder builder, IEnumerable<Regex> pathAndQueryMatching)
        {
            return builder.UseMiddleware<TrackAggregatedRequestUrls>(pathAndQueryMatching);
        }
    }
#endif
}
