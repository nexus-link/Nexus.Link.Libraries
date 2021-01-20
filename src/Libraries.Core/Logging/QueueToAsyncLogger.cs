using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Queue.Logic;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// A convenience class for logging.
    /// </summary>
    public class QueueToAsyncLogger : ISyncLogger
    {
        internal static readonly AsyncLocal<bool> AsyncLocalLoggingInProgress = new AsyncLocal<bool> { Value = false };

        private static bool LoggingInProgress
        {
            get => AsyncLocalLoggingInProgress.Value;
            set => AsyncLocalLoggingInProgress.Value = value;
        }

        private readonly IAsyncLogger _asyncLogger;
        private readonly MemoryQueue<LogQueueEnvelope> _queue;

        public TimeSpan KeepQueueAliveTimeSpan
        {
            get => _queue.KeepQueueAliveTimeSpan;
            set => _queue.KeepQueueAliveTimeSpan = value;
        }

        public QueueToAsyncLogger(IAsyncLogger asyncLogger)
        {
            _asyncLogger = asyncLogger;
            _queue = new MemoryQueue<LogQueueEnvelope>("LogQueue");
            _queue.SetQueueItemAction(LogFailSafeAsync);
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
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

            var envelope = new LogQueueEnvelope
            {
                LogRecord = logRecord,

                SavedContext = FulcrumApplication.Context.ValueProvider.SaveContext()
            };
            _queue.AddMessage(envelope);
        }

        /// <summary>
        /// Safe logging of messages. Will check for errors, but never throw an exception. If the log can't be made with the chosen logger, a fallback log will be created.
        /// </summary>
        /// <param name="envelope">An envelope with information about the logging.</param>
        private async Task LogFailSafeAsync(LogQueueEnvelope envelope)
        {
            LoggingInProgress = false;
            if (envelope?.LogRecord == null) return;
            var logRecord = envelope.LogRecord;
            try
            {
                FulcrumAssert.IsValidated(logRecord);
                FulcrumApplication.Context.ValueProvider.RestoreContext(envelope.SavedContext);
                var task = LogWithConfiguredLoggerFailSafeAsync(logRecord);
                AlsoLogWithTraceSourceInDevelopment(logRecord.SeverityLevel, logRecord);
                await task;
            }
            catch (Exception e)
            {
                LogHelper.FallbackToSimpleLoggingFailSafe(
                    $"{nameof(LogFailSafeAsync)} caught an exception.", logRecord, e);
            }
        }

        /// <summary>
        /// This is a property specifically for unit testing.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public bool OnlyForUnitTest_HasBackgroundWorkerForLogging
        {
            get
            {
                FulcrumAssert.IsTrue(FulcrumApplication.IsInDevelopment, null,
                    "This property must only be used in unit tests.");
                return _queue.OnlyForUnitTest_HasAliveBackgroundWorker;
            }
        }

        private static void AlsoLogWithTraceSourceInDevelopment(LogSeverityLevel severityLevel, LogRecord logRecord)
        {
            if (FulcrumApplication.Setup.SynchronousFastLogger?.GetType() == typeof(TraceSourceLogger)) return;
            if (!FulcrumApplication.IsInDevelopment) return;

            new TraceSourceLogger().SafeLog(severityLevel, logRecord.ToLogString(true));
        }

        private async Task LogWithConfiguredLoggerFailSafeAsync(LogRecord logRecord)
        {
            try
            {
                LoggingInProgress = true;
                await _asyncLogger.LogAsync(logRecord);
            }
            catch (Exception e)
            {
                LogHelper.FallbackToSimpleLoggingFailSafe(
                    $"{nameof(LogWithConfiguredLoggerFailSafeAsync)} caught an exception from logger {FulcrumApplication.Setup.SynchronousFastLogger?.GetType().FullName}.",
                    logRecord, e);
            }
            finally
            {
                LoggingInProgress = false;
            }
        }
    }
}

