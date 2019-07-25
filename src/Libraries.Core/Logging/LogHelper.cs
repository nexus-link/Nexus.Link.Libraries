using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Libraries.Core.Logging
{
    public static class LogHelper
    {
        /// <summary>
        /// Use this method to log when the original logging method fails.
        /// </summary>
        /// <param name="message">What went wrong with logging</param>
        /// <param name="logRecord">The message to log.</param>
        /// <param name="exception">If what went wrong had an exception</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        internal static void FallbackToSimpleLoggingFailSafe(string message, LogRecord logRecord,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (logRecord == null) return;
            try
            {
                var totalMessage = message == null ? "" : $"{message}\r";
                if (exception == null)
                {
                    totalMessage += "The logging mechanism itself failed and is using a fallback method.";
                }

                // If a message of warning or higher ends up here means it is critical, since this log will not end up in the normal log.
                var severityLevel = logRecord.IsGreaterThanOrEqualTo(LogSeverityLevel.Warning)
                    ? LogSeverityLevel.Critical
                    : LogSeverityLevel.Warning;
                string logRecordAsString;
                try
                {
                    logRecordAsString = JsonConvert.SerializeObject(logRecord);
                }
                catch (Exception)
                {
                    logRecordAsString = logRecord.ToLogString();
                }
                totalMessage += $"\r{logRecordAsString}";
                // ReSharper disable ExplicitCallerInfoArgument
                FallbackSafeLog(severityLevel, totalMessage, exception, memberName, filePath, lineNumber);
                // ReSharper restore ExplicitCallerInfoArgument
            }
            catch (Exception e)
            {
                FallbackSafeLog(LogSeverityLevel.Critical, $"Failed to log a message.", e);
            }
        }

        /// <summary>
        /// This logging is done with simple methods like local logging. The <paramref name="message"/> will be extended with application information and possibly exception information.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void FallbackSafeLog(
            LogSeverityLevel severityLevel,
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                var hideStackTrace = severityLevel < LogSeverityLevel.Error;
                // ReSharper disable ExplicitCallerInfoArgument
                var location = LocationToLogString(memberName, filePath, lineNumber);
                // ReSharper restore ExplicitCallerInfoArgument
                var messageWithLogInfo = $"{severityLevel}: {message}";
                messageWithLogInfo += $"\r{FulcrumApplication.ToLogString()}";
                messageWithLogInfo += ContextToLogString();
                messageWithLogInfo += $"\r{location}";
                if (exception != null) messageWithLogInfo += $"\r{exception.ToLogString(hideStackTrace)}";

                FulcrumApplication.Setup.FallbackLogger.SafeLog(severityLevel, messageWithLogInfo);
            }
            catch (Exception)
            {
                // We give up
            }
        }

        public static string ContextToLogString()
        {
            var message = "";
            try
            {
                var correlationId = FulcrumApplication.Context.CorrelationId;
                if (!string.IsNullOrWhiteSpace(correlationId)) message += $"\rCorrelation id: {correlationId}";
            }
            catch (Exception)
            {
                // Ignore
            }
            try
            {
                var clientName = FulcrumApplication.Context.CallingClientName;
                if (!string.IsNullOrWhiteSpace(clientName)) message += $"\rClient name: {clientName}";
            }
            catch (Exception)
            {
                // Ignore
            }
            try
            {
                var clientTenant = FulcrumApplication.Context.ClientTenant;
                if (clientTenant != null) message += $"\rClient tenant: {clientTenant.ToLogString()}";
            }
            catch (Exception)
            {
                // Ignore
            }

            return message;
        }

        public static string LocationToLogString([CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) => $"at {memberName} in {filePath}: line {lineNumber}";

        /// <summary>
        /// Recommended <see cref="ISyncLogger"/> for unit testing.
        /// </summary>
        public static ISyncLogger RecommendedSyncLoggerForUnitTest { get; } = new ConsoleLogger();

        /// <summary>
        /// Recommended <see cref="ISyncLogger"/> for developing an application. For test environments and production, we recommend that you write your own logger.
        /// </summary>
        public static ISyncLogger RecommendedSyncLoggerForRuntime { get; } = new TraceSourceLogger();

        /// <summary>
        /// Recommended <see cref="ISyncLogger"/> for unit testing.
        /// </summary>
        public static IFallbackLogger RecommendedFallbackLoggerForUnitTest { get; } = new ConsoleLogger();

        /// <summary>
        /// Recommended <see cref="ISyncLogger"/> for developing an application. For test environments and production, we recommend that you write your own logger.
        /// </summary>
        public static IFallbackLogger RecommendedFallbackLoggerForRuntime { get; } = new TraceSourceLogger();
    }
}
