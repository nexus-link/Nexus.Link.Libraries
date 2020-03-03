#if NETCOREAPP
#else

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Nexus.Link.Libraries.Core.Telemetry;

namespace Nexus.Link.Libraries.Azure.Web.AspNet.Telemetry
{
    public class ApplicationInsightsTelemetryHandler : ITelemetryHandler
    {
        public TelemetryClient TelemetryClient { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/azure/azure-monitor/app/api-custom-events-metrics</remarks>
        public ApplicationInsightsTelemetryHandler(string contextDeviceId = null, string contextUserId = null)
        {
            // TODO: Do we need configuration?
            TelemetryClient = new TelemetryClient();

            if (!string.IsNullOrWhiteSpace(contextDeviceId))
            {
                TelemetryClient.Context.Device.Id = contextDeviceId;
            }
            if (!string.IsNullOrWhiteSpace(contextUserId))
            {
                TelemetryClient.Context.User.Id = contextUserId;
            }
        }

        /// <inheritdoc />
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            TelemetryClient.TrackEvent(eventName, properties, metrics);
        }

        /// <inheritdoc />
        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            TelemetryClient.TrackException(exception, properties, metrics);
        }

        /// <inheritdoc />
        public void TrackTrace(string message, IDictionary<string, string> properties = null)
        {
            TelemetryClient.TrackTrace(message, properties);
        }
    }
}
#endif