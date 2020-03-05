#if NETCOREAPP
#else

using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
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
            FulcrumApplication.ValidateButNotInProduction();

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
            try
            {
                TelemetryClient.TrackEvent(eventName, properties, metrics);
            }
            catch (Exception e)
            {
                Log.LogWarning($"Could not track event: {e.Message}", e);
            }
        }

        /// <inheritdoc />
        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            try
            {
                TelemetryClient.TrackException(exception, properties, metrics);
            }
            catch (Exception e)
            {
                Log.LogWarning($"Could not track exception: {e.Message}", e);
            }
        }

        /// <inheritdoc />
        public void TrackTrace(string message, IDictionary<string, string> properties = null)
        {
            try
            {
                TelemetryClient.TrackTrace(message, properties);
            }
            catch (Exception e)
            {
                Log.LogWarning($"Could not track trace: {e.Message}", e);
            }
        }
    }
}
#endif