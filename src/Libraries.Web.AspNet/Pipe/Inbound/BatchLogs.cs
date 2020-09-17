using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    /// <summary>
    /// Adds a <see cref="BatchLogger.StartBatch"/> before the call and a <see cref="BatchLogger.EndBatch"/>  after the call.
    /// To use this, you have to add the <see cref="BatchLogger"/> as your FulcrumApplication.Setup.SynchronousFastLogger.
    /// </summary>
    public class BatchLogs : CompatibilityDelegatingHandler
    {
        private readonly LogSeverityLevel _logAllThreshold;
        private readonly bool _releaseRecordsAsLateAsPossible;
        private static readonly DelegateState DelegateState = new DelegateState(typeof(BatchLogs).FullName);

        /// <summary>
        /// True if this delegate has started in the current context
        /// </summary>
        public static bool HasStarted
        {
            get => DelegateState.HasStarted;
            private set => DelegateState.HasStarted = value;
        }

#if NETCOREAPP
        /// <summary>
        /// This handler makes sure that if one log message in a batch has a severity level
        /// equal to or higher than <paramref name="logAllThreshold"/>, then all the logs within that
        /// batch will be logged, regardless of the value of <see cref="ApplicationSetup.LogSeverityLevelThreshold"/>.
        /// </summary>
        /// <param name="next">The inner handler</param>
        /// <param name="logAllThreshold">The threshold for logging all messages within a batch.</param>
        /// <param name="releaseRecordsAsLateAsPossible">True means that the records will be released at the end of the batch.
        /// False means that they will be released as soon as one message hits the threshold and then all messages will be released instantly until the batch ends.</param>
        public BatchLogs(RequestDelegate next,
            LogSeverityLevel logAllThreshold = LogSeverityLevel.Error, bool releaseRecordsAsLateAsPossible = false)
            : base(next)
        {
            _logAllThreshold = logAllThreshold;
            _releaseRecordsAsLateAsPossible = releaseRecordsAsLateAsPossible;
        }
#else
        /// <summary>
        /// This handler makes sure that if one log message in a batch has a severity level
        /// equal to or higher than <paramref name="logAllThreshold"/>, then all the logs within that
        /// batch will be logged, regardless of the value of <see cref="ApplicationSetup.LogSeverityLevelThreshold"/>.
        /// </summary>
        /// <param name="logAllThreshold">The threshold for logging all messages within a batch.</param>
        /// <param name="releaseRecordsAsLateAsPossible">True means that the records will be released at the end of the batch.
        /// False means that they will be released as soon as one message hits the threshold and then all messages will be released instantly until the batch ends.</param>
        public BatchLogs(LogSeverityLevel logAllThreshold = LogSeverityLevel.Error,
            bool releaseRecordsAsLateAsPossible = false)
        {
            _logAllThreshold = logAllThreshold;
            _releaseRecordsAsLateAsPossible = releaseRecordsAsLateAsPossible;
        }
#endif
        /// <inheritdoc />
        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            InternalContract.Require(!LogRequestAndResponse.HasStarted,
                $"{nameof(LogRequestAndResponse)} must not precede {nameof(SaveCorrelationId)}");
            InternalContract.Require(!ExceptionToFulcrumResponse.HasStarted,
                $"{nameof(ExceptionToFulcrumResponse)} must not precede {nameof(SaveCorrelationId)}");
            HasStarted = true;
            BatchLogger.StartBatch(_logAllThreshold, _releaseRecordsAsLateAsPossible);
            try
            {
                await CallNextDelegateAsync(context);
            }
            finally
            {
                BatchLogger.EndBatch();
            }
        }
    }
#if NETCOREAPP
    public static class BatchLogsExtension
    {
        public static IApplicationBuilder UseNexusBatchLogs(
            this IApplicationBuilder builder,
            LogSeverityLevel logAllThreshold = LogSeverityLevel.Error, bool releaseRecordsAsLateAsPossible = false)
        {
            return builder.UseMiddleware<BatchLogs>(logAllThreshold, releaseRecordsAsLateAsPossible);
        }
    }
#endif
}
