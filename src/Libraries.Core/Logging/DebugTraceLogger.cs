using System;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public class DebugTraceLogger : ISyncLogger, IFallbackLogger
    {
        /// <inheritdoc />
        public void SafeLog(LogSeverityLevel logSeverityLevel, string message)
        {
            try
            {
                if (logSeverityLevel == LogSeverityLevel.None) return;
                if (logSeverityLevel >= LogSeverityLevel.Warning)
                {
                    System.Diagnostics.Trace.WriteLine($"\r{logSeverityLevel} {message}\r");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"\r{logSeverityLevel} {message}\r");
                }
            }
            catch (Exception)
            {
                // This method must never fail.
            }
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            SafeLog(logRecord.SeverityLevel, logRecord.ToLogString());
        }
    }
}

