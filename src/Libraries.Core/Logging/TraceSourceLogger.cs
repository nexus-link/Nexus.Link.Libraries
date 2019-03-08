using System;
using System.Diagnostics;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public class TraceSourceLogger : ISyncLogger, IFallbackLogger
    {
        private static readonly TraceSource TraceSource = new TraceSource("FulcrumTraceSource");

        /// <inheritdoc />
        public void SafeLog(LogSeverityLevel logSeverityLevel, string message)
        {
            try
            {
                if (logSeverityLevel == LogSeverityLevel.None) return;

                message = $"[{DateTimeOffset.Now}] {message}";
                TraceEventType eventType;
                switch (logSeverityLevel)
                {
                    case LogSeverityLevel.Verbose:
                        eventType = TraceEventType.Verbose;
                        break;
                    case LogSeverityLevel.Information:
                        eventType = TraceEventType.Information;
                        break;
                    case LogSeverityLevel.Warning:
                        eventType = TraceEventType.Warning;
                        break;
                    case LogSeverityLevel.Error:
                        eventType = TraceEventType.Error;
                        break;
                    case LogSeverityLevel.Critical:
                        eventType = TraceEventType.Critical;
                        break;
                    // ReSharper disable once RedundantCaseLabel
                    case LogSeverityLevel.None:
                    default:
                        TraceSource.TraceEvent(TraceEventType.Critical, 0,
                            $"Unexpected {nameof(logSeverityLevel)} ({logSeverityLevel}) for message:\r{message}.");
                        return;
                }

                TraceSource.TraceEvent(eventType, 0, $"\r{message}\r");
            }
            catch (Exception)
            {
                // This method should never fail.
            }
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            SafeLog(logRecord.SeverityLevel, logRecord.ToLogString());
        }
    }
}

