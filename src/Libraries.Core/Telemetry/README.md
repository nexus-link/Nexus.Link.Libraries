# Telemetry

With the [ITelemetryHandler](ITelemetryHandler.cs) interface you can

* Send custom events with properties and metrics
* Track exceptions
* Send trace messages

## Implementations

* For development, you can use [TraceTelemetryHandler](TraceTelemetryHandler.cs)
* If you are using Application Insights, use [ApplicationInsightsTelemetryHandler](../../Libraries.Azure.Web.AspNet/Telemetry/ApplicationInsightsTelemetryHandler.cs)

