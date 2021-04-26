#if NETCOREAPP
#else

using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Nexus.Link.Libraries.Azure.Web.AspNet.ApplicationInsights.Initializers
{
    /// <summary>
    /// Use in Global.asax.cs like to add REFERRER to request logging in Application Insights
    /// <code>
    /// TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["iKey"];
    /// TelemetryConfiguration.Active.TelemetryInitializers.Add(new RefererTelemetryInitializer());
    /// </code>
    /// </summary>
    public class RefererTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;
            if (requestTelemetry == null) return;

            var referrer = HttpContext.Current?.Request.UrlReferrer?.ToString();
            if (!string.IsNullOrWhiteSpace(referrer))
            {
                telemetry.Context.GlobalProperties["Referrer"] = referrer;
            }
        }
    }
}

#endif