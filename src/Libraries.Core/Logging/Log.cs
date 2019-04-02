using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;

// ReSharper disable ExplicitCallerInfoArgument

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public static class Log
    {
        internal static readonly AsyncLocal<bool> AsyncLocalLoggingInProgress = new AsyncLocal<bool> {Value = false};

        private static bool LoggingInProgress
        {
            get => AsyncLocalLoggingInProgress.Value;
            set => AsyncLocalLoggingInProgress.Value = value;
        }

        private static bool _applicationValidated;

        /// <summary>
        /// Verbose logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogVerbose(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Verbose, message, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Verbose logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="data">Additional data associated with this log message.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogVerbose(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Verbose, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Information logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogInformation(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Information, message, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Information logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="data">Additional data associated with this log message.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogInformation(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Information, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Warning logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogWarning(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Warning, message, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Warning logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="data">Additional data associated with this log message.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogWarning(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Warning, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Error logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogError(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Error, message, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Error logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="data">Additional data associated with this log message.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogError(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Error, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Critical logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogCritical(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Critical, message, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Critical logging of <paramref name="message"/> and optional <paramref name="exception"/>.
        /// </summary>
        /// <param name="message">The message to print.</param>
        /// <param name="data">Additional data associated with this log message.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogCritical(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Critical, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Safe logging of a message. Will check for errors, but never throw an exception. If the log can't be made with the chosen logger, a fallback log will be created.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogOnLevel(
            LogSeverityLevel severityLevel,
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(severityLevel, message, (JObject)null, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Safe logging of a message. Will check for errors, but never throw an exception. If the log can't be made with the chosen logger, a fallback log will be created.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="data">Additional data associated with this log message.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogOnLevel(
            LogSeverityLevel severityLevel,
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            JObject jObject;
            try
            {
                jObject = data == null ? null : JObject.FromObject(data);
            }
            catch (Exception e)
            {
                jObject = new JObject(new { Message = $"Failed to convert log data from type object ({data?.GetType().FullName}) to type JObject: {e.Message}" });
            }
            LogOnLevel(severityLevel, message, jObject, exception, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Safe logging of a message. Will check for errors, but never throw an exception. If the log can't be made with the chosen logger, a fallback log will be created.
        /// </summary>
        /// <param name="severityLevel">The severity level for this log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="data">Additional data associated with this log message.</param>
        /// <param name="exception">An optional exception that will have it's information incorporated in the message.</param>
        /// <param name="memberName">Method or property name of the caller</param>
        /// <param name="filePath">Full path of the source file that contains the caller. This is the file path at compile time.</param>
        /// <param name="lineNumber">Line number in the source file at which the method is called</param>
        public static void LogOnLevel(
            LogSeverityLevel severityLevel,
            string message,
            JObject data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!_applicationValidated)
            {
                FulcrumApplication.ValidateButNotInProduction();
                _applicationValidated = true;
            }

            var logRecord = CreateLogInstanceInformation(severityLevel, message, data, exception, memberName, filePath, lineNumber);
            if (LoggingInProgress)
            {
                // Recursive logging detected
                if (logRecord.IsGreaterThanOrEqualTo(LogSeverityLevel.Warning))
                {
                    const string abortMessage =
                        "Log recursion! Will send the following inner log to the fallback logger instead of the configured logger.";
                    LogHelper.FallbackToSimpleLoggingFailSafe(abortMessage, logRecord);
                }

                return;
            }

            LoggingInProgress = true;
            try
            {
                FulcrumApplication.Setup.SynchronousFastLogger.LogSync(logRecord);
            }
            catch (Exception e)
            {
                var typeName = FulcrumApplication.Setup.SynchronousFastLogger.GetType().FullName;
                LogHelper.FallbackToSimpleLoggingFailSafe($"The configured logging method ({typeName}.{nameof(ISyncLogger.LogSync)}) failed.", logRecord, e);
            }
            finally
            {
                LoggingInProgress = false;
            }
        }

        internal static LogRecord CreateLogInstanceInformation(
            LogSeverityLevel severityLevel,
            string message,
            JObject data,
            Exception exception,
            string memberName = "",
            string filePath = "",
            int lineNumber = 0)
        {
            var logInstanceInformation = new LogRecord
            {
                TimeStamp = DateTimeOffset.Now,
                SeverityLevel = severityLevel,
                Message = message,
                Data = data,
                Location = LogHelper.LocationToLogString(memberName, filePath, lineNumber),
                Exception = exception
            };

            if (logInstanceInformation.IsGreaterThanOrEqualTo(LogSeverityLevel.Error) && exception == null)
            {
                logInstanceInformation.StackTrace = Environment.StackTrace;
            }

            return logInstanceInformation;
        }
    }
}