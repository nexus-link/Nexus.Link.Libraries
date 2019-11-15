using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Threads;

namespace Nexus.Link.Libraries.Core.Application
{
    /// <summary>
    /// A class with settings that are expected from in an application that uses Fulcrum libraries.
    /// </summary>
    public class ApplicationSetup : IValidatable
    {
        /// <summary>
        /// The name of the application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current <see cref="RunTimeLevel"/> of the application. Affects logging, testing, etc.
        /// </summary>
        /// <remarks>For a multi tenant application, this is the run time level for the application itself, not it's tenants.</remarks>
        public RunTimeLevelEnum RunTimeLevel { get; set; }

        /// <summary>
        /// The tenant for the application. For a multi-tenant application, this is the application tenant not any caller tenant.
        /// </summary>
        public Tenant Tenant { get; set; }

        /// <summary>
        /// How to deal with background threads.
        /// </summary>
        public IThreadHandler ThreadHandler { get; set; }

        /// <summary>
        /// The logger to use for logging for the entire application.
        /// </summary>
        [Obsolete("Use SyncLogger or AsyncLogger.", true)]
        public IFulcrumFullLogger FullLogger { get; set; }

        /// <summary>
        /// Set this to a fast synchronous logger.
        /// </summary>
        /// <remarks>If you have an asynchronous logger (fast or slow), then you can use <see cref="QueueToAsyncLogger"/> as a fast queue that will feed your logger.</remarks>
        public ISyncLogger SynchronousFastLogger { get; set; }

        /// <summary>
        /// A logger to use when the normal logger fails.
        /// </summary>
        public IFallbackLogger FallbackLogger { get; set; }

        /// <summary>
        /// The context value provider that will be used all over the application.
        /// </summary>
        [Obsolete("Use FulcrumApplication.Context.ValueProvider.", true)]
        public IContextValueProvider ContextValueProvider { get; set; }

        /// <summary>
        /// If any log in a batch of logs has a severity level that is equal to or higher than this level,
        /// then <see cref="LogSeverityLevelThreshold"/> is overriden and all logs will be used.
        /// </summary>
        [Obsolete("Use BatchLogger and pick a value in the constructor.", true)]
        public LogSeverityLevel BatchLogAllSeverityLevelThreshold { get; set; }

        /// <summary>
        /// A log must have at least this level to be sent for logging. Can be overridden in batches by <see cref="BatchLogAllSeverityLevelThreshold"/>.
        /// </summary>
        public LogSeverityLevel LogSeverityLevelThreshold{ get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(Name, nameof(Name), errorLocation);
            FulcrumValidate.AreNotEqual(RunTimeLevelEnum.None, RunTimeLevel, nameof(RunTimeLevel), errorLocation);
            FulcrumValidate.IsValidated(Tenant, propertyPath, nameof(Tenant), errorLocation);
            FulcrumValidate.IsNotNull(ThreadHandler, nameof(ThreadHandler), errorLocation);
            FulcrumValidate.IsNotNull(SynchronousFastLogger, nameof(SynchronousFastLogger), errorLocation);
            FulcrumValidate.IsNotNull(FallbackLogger, nameof(FallbackLogger), errorLocation);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} {Tenant} ({RunTimeLevel})";
        }
    }
}