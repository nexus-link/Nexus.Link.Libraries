using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;

namespace Nexus.Link.Libraries.Core.Telemetry
{
    /// <summary>
    /// For development purposes.
    /// </summary>
    /// <remarks>
    /// Uses Trace.WriteLine to print out telemetry.
    /// </remarks>
    public class TraceTelemetryHandler : ITelemetryHandler
    {
        private static int _eventCounter;
        private static int _exceptionCounter;

        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var count = Interlocked.Increment(ref _eventCounter);
            var props = SerializeExtendedProperties(properties);
            var metricsString = SerializeMetrics(metrics);

            Trace.WriteLine($"[Telemetry:Event:{eventName}:{count}] {props}");
            if (!string.IsNullOrWhiteSpace(metricsString))
            {
                Trace.WriteLine($"[Telemetry:Event:{eventName}:{count}] {metricsString}");
            }
        }

        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var count = Interlocked.Increment(ref _exceptionCounter);
            var props = SerializeExtendedProperties(properties);
            var metricsString = SerializeMetrics(metrics);

            Trace.WriteLine($"[Telemetry:Exception:{count}] {props}");
            if (!string.IsNullOrWhiteSpace(metricsString))
            {
                Trace.WriteLine($"[Telemetry:Exception:{count}] {metricsString}");
            }
            Trace.WriteLine($"[Telemetry:Exception:{count}] {exception.Message}");
            Trace.WriteLine($"[Telemetry:Exception:{count}] {exception.StackTrace}");
        }

        public void TrackTrace(string message, IDictionary<string, string> properties = null)
        {
            var props = SerializeExtendedProperties(properties, new KeyValuePair<string, string>("_message", message));
            Trace.WriteLine($"[Telemetry:Trace] {props}");
        }

        private static string SerializeExtendedProperties(IDictionary<string, string> properties, params KeyValuePair<string, string>[] extra)
        {
            var extendedProperties = new Dictionary<string, string>(properties ?? new Dictionary<string, string>())
            {
                {"_timestamp", DateTimeOffset.Now.ToString()}
            };

            if (extra != null)
            {
                foreach (var keyValuePair in extra)
                {
                    extendedProperties.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }

            var props = JsonConvert.SerializeObject(extendedProperties, Formatting.Indented);
            return props;
        }

        private static string SerializeMetrics(IDictionary<string, double> metrics)
        {
            if (metrics == null) return "";

            var metricsString = JsonConvert.SerializeObject(metrics, Formatting.Indented);
            return metricsString;
        }

    }
}
