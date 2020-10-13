using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Libraries.Core.Logging
{
    /// <summary>
    /// Only forwards records that are over a threshold severity level, with one exception;
    /// if one of the logs in a batch of logs are over another threshold value, all logs are forwarded.
    /// </summary>
    public class BatchLogger : ISyncLogger
    {
        private static ISyncLogger _syncLogger;
        private static readonly AsyncLocal<BatchInfo> AsyncLocalBatch = new AsyncLocal<BatchInfo>();

        private static BatchInfo Batch => AsyncLocalBatch.Value;

        private static bool HasStarted => Batch != null;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="syncLogger">The logger to use when a batch has finished.</param>
        /// <remarks>The <paramref name="syncLogger"/> is stored in a static variable, so you can't
        /// have two batch loggers running at the same time and expect them to log to different loggers</remarks>
        public BatchLogger(ISyncLogger syncLogger)
        {
            _syncLogger = syncLogger;
        }

        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            // The goal is to enqueue the record, but there are exceptions to this.

            if (!HasStarted || Batch.EndBatchRequested)
            {
                _syncLogger.LogSync(logRecord);
                return;
            }

            if (!Batch.HasReachedThreshold &&
                Batch.LogAllThreshold != LogSeverityLevel.None &&
                logRecord.IsGreaterThanOrEqualTo(Batch.LogAllThreshold))
            {
                Batch.HasReachedThreshold = true;
                if (!Batch.ReleaseRecordsAsLateAsPossible) FlushValues();
            }

            if (!Batch.ReleaseRecordsAsLateAsPossible && Batch.HasReachedThreshold)
            {
                _syncLogger.LogSync(logRecord);
            }
            else
            {
                Batch.LogRecords.Add(logRecord);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logIndividualThreshold">Only log log records that has this or higher severity level. Can be overruled by <paramref name="logAllThreshold"/>.</param>
        /// <param name="logAllThreshold">If any of the log records in the batch has this or higher severity level,
        /// the <paramref name="logIndividualThreshold"/> will be ignored and consequently all log records will be logged.
        /// Set this to <see cref="LogSeverityLevel.None"/> to avoid this functionality, i.e. only <paramref name="logIndividualThreshold"/> will be used.</param>
        /// <param name="releaseRecordsAsLateAsPossible">This is relevant when a log record is logged that fulfills the <paramref name="logAllThreshold"/> threshold.
        /// If true, the logs in the batch will not be released until <see cref="EndBatch"/> is called. If false, then the current log records will be immediately released
        /// and the following individual logs in the batch will be release immediately.</param>
        [Obsolete("The parameter logIndividualThreshold has been removed. Use the application setting FulcrumApplication.Setup.LogSeverityLevelThreshold instead. Obsolete from 2019-06-27.", true)]
        public static void StartBatch(
            LogSeverityLevel logIndividualThreshold = LogSeverityLevel.Warning,
            LogSeverityLevel logAllThreshold = LogSeverityLevel.Error,
            bool releaseRecordsAsLateAsPossible = false)
        {
            AsyncLocalBatch.Value = new BatchInfo();
            Batch.LogRecords = new List<LogRecord>();
            Batch.ContextId = FulcrumApplication.Context.ContextId;
            Batch.HasReachedThreshold = false;
            Batch.LogAllThreshold = logAllThreshold;
            Batch.ReleaseRecordsAsLateAsPossible = releaseRecordsAsLateAsPossible;
            FulcrumApplication.Context.IsInBatchLogger = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logAllThreshold">If any of the log records in the batch has this or higher severity level,
        /// the <see cref="ApplicationSetup.LogSeverityLevelThreshold"/> will be ignored and consequently all log records will be logged.
        /// Set this to <see cref="LogSeverityLevel.None"/> to avoid this functionality, i.e. only <see cref="ApplicationSetup.LogSeverityLevelThreshold"/> will be used.</param>
        /// <param name="releaseRecordsAsLateAsPossible">This is relevant when a log record is logged that fulfills the <paramref name="logAllThreshold"/> threshold.
        /// If true, the logs in the batch will not be released until <see cref="EndBatch"/> is called. If false, then the current log records will be immediately released
        /// and the following individual logs in the batch will be release immediately.</param>
        public static void StartBatch(
            LogSeverityLevel logAllThreshold = LogSeverityLevel.Error,
            bool releaseRecordsAsLateAsPossible = false)
        {
            AsyncLocalBatch.Value = new BatchInfo();
            Batch.LogRecords = new List<LogRecord>();
            Batch.ContextId = FulcrumApplication.Context.ContextId;
            Batch.HasReachedThreshold = false;
            Batch.LogAllThreshold = logAllThreshold;
            Batch.ReleaseRecordsAsLateAsPossible = releaseRecordsAsLateAsPossible;
            FulcrumApplication.Context.IsInBatchLogger = true;
        }


        public static void EndBatch()
        {
            if (!HasStarted) return;
            Batch.EndBatchRequested = true;

            // Note! Important that this is set before flushing: We depend on Log.LogOnLevel to discard low severity logs
            FulcrumApplication.Context.IsInBatchLogger = false;

            FlushValues();
            AsyncLocalBatch.Value = null;
        }

        public static void FlushValues()
        {
            if (!HasStarted || Batch.LogRecords == null || !Batch.LogRecords.Any()) return;

            foreach (var logRecord in PickRelevantLogRecords())
            {
                _syncLogger.LogSync(logRecord);
            }
            Batch.LogRecords = new List<LogRecord>();
        }

        private static IEnumerable<LogRecord> PickRelevantLogRecords()
        {
            return Batch.HasReachedThreshold
                ? Batch.LogRecords
                : Batch.LogRecords.Where(lr => lr.IsGreaterThanOrEqualTo(FulcrumApplication.Setup.LogSeverityLevelThreshold));
        }

        private class BatchInfo
        {
            public LogSeverityLevel LogAllThreshold { get; set; }
            public bool ReleaseRecordsAsLateAsPossible { get; set; }
            public List<LogRecord> LogRecords { get; set; }
            public Guid ContextId { get; set; }
            public bool HasReachedThreshold { get; set; }

            /// <summary>
            /// Tells if the process of ending the batch has started.
            /// This means that queuing within the <see cref="BatchLogger"/> is prohibited.
            /// </summary>
            public bool EndBatchRequested { get; set; }
        }
    }
}
