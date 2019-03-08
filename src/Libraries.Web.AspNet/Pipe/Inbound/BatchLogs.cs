using System.Threading.Tasks;
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
        private readonly LogSeverityLevel _logIndividualThreshold;
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
        /// <inheritdoc />
        public BatchLogs(RequestDelegate next,
            LogSeverityLevel logIndividualThreshold = LogSeverityLevel.Warning,
            LogSeverityLevel logAllThreshold = LogSeverityLevel.Error, bool releaseRecordsAsLateAsPossible = false)
        :base(next)
        {
            _logIndividualThreshold = logIndividualThreshold;
            _logAllThreshold = logAllThreshold;
            _releaseRecordsAsLateAsPossible = releaseRecordsAsLateAsPossible;
        }
#else
        public BatchLogs(LogSeverityLevel logIndividualThreshold = LogSeverityLevel.Warning,
            LogSeverityLevel logAllThreshold = LogSeverityLevel.Error, bool releaseRecordsAsLateAsPossible = false)
        {
            _logIndividualThreshold = logIndividualThreshold;
            _logAllThreshold = logAllThreshold;
            _releaseRecordsAsLateAsPossible = releaseRecordsAsLateAsPossible;
        }
#endif
        /// <inheritdoc />
        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            InternalContract.Require(!LogRequestAndResponse.HasStarted, $"{nameof(LogRequestAndResponse)} must not precede {nameof(BatchLogs)}");
            HasStarted = true;
            BatchLogger.StartBatch(_logIndividualThreshold, _logAllThreshold, _releaseRecordsAsLateAsPossible);
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
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BatchLogs>();
        }
    }
#endif
}
