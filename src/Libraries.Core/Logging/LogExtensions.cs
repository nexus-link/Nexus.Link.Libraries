using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// Extension methods regarding logging
    /// </summary>
    public static class LogExtensions
    {
        public static LogLevel ToLogLevel(this LogSeverityLevel severityLevel)
        {
            switch (severityLevel)
            {
                case LogSeverityLevel.None:
                    return LogLevel.None;
                case LogSeverityLevel.Verbose:
                    return LogLevel.Debug;
                case LogSeverityLevel.Information:
                    return LogLevel.Information;
                case LogSeverityLevel.Warning:
                    return LogLevel.Warning;
                case LogSeverityLevel.Error:
                    return LogLevel.Error;
                case LogSeverityLevel.Critical:
                    return LogLevel.Critical;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severityLevel), severityLevel, null);
            }
        }
        public static LogSeverityLevel ToLogSeverityLevel(this LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.None:
                    return LogSeverityLevel.None;
                case LogLevel.Trace:
                case LogLevel.Debug:
                    return LogSeverityLevel.Verbose;
                case LogLevel.Information:
                    return LogSeverityLevel.Information;
                case LogLevel.Warning:
                    return LogSeverityLevel.Warning;
                case LogLevel.Error:
                    return LogSeverityLevel.Error;
                case LogLevel.Critical:
                    return LogSeverityLevel.Critical;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
    }
}
