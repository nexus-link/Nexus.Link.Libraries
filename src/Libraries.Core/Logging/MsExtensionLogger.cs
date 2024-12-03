using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public class MsExtensionLogger(ILogger<MsExtensionLogger> logger) : ISyncLogger, IFallbackLogger
    {
        /// <inheritdoc />
        public void SafeLog(LogSeverityLevel logSeverityLevel, string message)
        {
            PrivateSafeLog(logSeverityLevel, message, null);
        }

        private void PrivateSafeLog(LogSeverityLevel logSeverityLevel, string message, Exception exception = null)
        {
            try
            {
                LogLevel logLevel;
                switch (logSeverityLevel)
                {
                    case LogSeverityLevel.Verbose:
                        logLevel = LogLevel.Trace;
                        break;
                    case LogSeverityLevel.Information:
                        logLevel = LogLevel.Information;
                        break;
                    case LogSeverityLevel.Warning:
                        logLevel = LogLevel.Warning;
                        break;
                    case LogSeverityLevel.Error:
                        logLevel = LogLevel.Error;
                        break;
                    case LogSeverityLevel.Critical:
                        logLevel = LogLevel.Critical;
                        break;
                    // ReSharper disable once RedundantCaseLabel
                    case LogSeverityLevel.None:
                        logLevel = LogLevel.None;
                        break;
                    default:
                        logger.LogCritical($"Unexpected {nameof(logSeverityLevel)} ({logSeverityLevel}) for message:\r{message}.");
                        return;
                }
                logger.Log(logLevel, exception, message);
            }
            catch (Exception)
            {
                // This method should never fail.
            }
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            PrivateSafeLog(logRecord.SeverityLevel, ToLogString(logRecord), logRecord.Exception);
        }

        private string ToLogString(LogRecord logRecord)
        {
            var correlationId = FulcrumApplication.Context.CorrelationId;
            var stringBuilder = new StringBuilder(logRecord.TimeStamp.ToLogString());
            if (!string.IsNullOrWhiteSpace(correlationId)) stringBuilder.Append($" {correlationId}");
            stringBuilder.Append(" ");
            stringBuilder.Append(logRecord.SeverityLevel.ToString());
            var context = ContextToLogString();
            if (!string.IsNullOrWhiteSpace(context))
            {
                stringBuilder.Append(" ");
                stringBuilder.AppendLine(context);
            }
            else
            {
                stringBuilder.AppendLine("");
            }
            stringBuilder.Append(logRecord.Message);
            return stringBuilder.ToString();
        }

        private string ContextToLogString()
        {
            var stringBuilder = new StringBuilder($"{FulcrumApplication.Setup.Tenant} {FulcrumApplication.Setup.Name} ({FulcrumApplication.Setup.RunTimeLevel}) context:{ FulcrumApplication.Context.ContextId}");

            var clientTenant = FulcrumApplication.Context.ClientTenant;
            if (clientTenant != null)
            {
                stringBuilder.Append(" tenant: ");
                stringBuilder.Append(clientTenant.ToLogString());
            }

            var clientName = FulcrumApplication.Context.CallingClientName;
            if (!string.IsNullOrWhiteSpace(clientName))
            {
                stringBuilder.Append(" client: ");
                stringBuilder.Append(clientName);
            }
            return stringBuilder.ToString();
        }
    }
}

