using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;

// ReSharper disable ExplicitCallerInfoArgument

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public class LogFacade : ILogFacade
    {
        internal static readonly AsyncLocal<bool> AsyncLocalLoggingInProgress = new AsyncLocal<bool> {Value = false};

        private static bool LoggingInProgress
        {
            get => AsyncLocalLoggingInProgress.Value;
            set => AsyncLocalLoggingInProgress.Value = value;
        }

        private bool _applicationValidated;

        /// <inheritdoc />
        public void LogVerbose(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Verbose, message, exception, memberName, filePath, lineNumber);
            var logger = new LogFacade();
            logger.LogInformation(null, new Exception("test"), "My test message", this, this);
        }

        /// <inheritdoc />
        public void LogVerbose(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Verbose, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogInformation(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Information, message, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogInformation(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Information, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Warning, message, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogWarning(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Warning, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogError(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Error, message, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogError(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Error, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Critical, message, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogCritical(
            string message,
            object data,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogOnLevel(LogSeverityLevel.Critical, message, data, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogOnLevel(
            LogSeverityLevel severityLevel,
            string message,
            Exception exception = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            // ReSharper disable once RedundantCast
            LogOnLevel(severityLevel, message, (JObject)null, exception, memberName, filePath, lineNumber);
        }

        /// <inheritdoc />
        public void LogOnLevel(
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

        /// <inheritdoc />
        public void LogOnLevel(
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

            if (!FulcrumApplication.Context.IsInBatchLogger &&
                !logRecord.IsGreaterThanOrEqualTo(FulcrumApplication.Setup.LogSeverityLevelThreshold))
            {
                // Discard this log as its severity level is too low
                return;
            }

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
            [CallerLineNumber] int lineNumber = 0)
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

            if (logInstanceInformation.IsGreaterThanOrEqualTo(LogSeverityLevel.Error))
            {
                logInstanceInformation.StackTrace = Environment.StackTrace;
            }

            return logInstanceInformation;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = SafeFormatMessage(state, exception, formatter);
            LogOnLevel(logLevel.ToLogSeverityLevel(), message, state, exception);
        }

        private static string SafeFormatMessage<TState>(TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string message;
            try
            {
                if (formatter != null)
                {
                    message = formatter(state, exception);
                }
                else if (state is ILoggable loggable)
                {
                    message = loggable.ToLogString();
                }
                else
                {
                    message = JsonConvert.SerializeObject(state);
                }
            }
            catch (Exception)
            {
                message = state.ToString();
            }

            return message;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {

            if (FulcrumApplication.Context.IsInBatchLogger) return true;
            return logLevel >= FulcrumApplication.Setup.LogSeverityLevelThreshold.ToLogLevel();
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string ToLogString()
        {
            throw new NotImplementedException();
        }
    }
}