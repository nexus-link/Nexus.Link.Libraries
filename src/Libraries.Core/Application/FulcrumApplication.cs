using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform;
using Nexus.Link.Libraries.Core.Threads;

namespace Nexus.Link.Libraries.Core.Application
{
    /// <summary>
    /// Help class to setup your application
    /// </summary>
    public static class FulcrumApplication
    {
        /// <summary>
        /// The setup for the fulcrum application.
        /// </summary>
        public static readonly ApplicationSetup Setup = new ApplicationSetup();

        /// <summary>
        /// Use this to get application settings.
        /// </summary>
        public static AppSettings AppSettings { get; set; }

        /// <summary>
        /// Initialize <see cref="Setup"/>.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="tenant">The tenant that the application itself runs in.</param>
        /// <param name="level">The run time level for the application itself.</param>
        /// <remarks>Will setup all mandatory fields for <see cref="Setup"/>, but you might want to override those values when this method returns."/></remarks>
        public static void Initialize(string name, Tenant tenant, RunTimeLevelEnum level)
        {
            InternalContract.RequireNotNullOrWhiteSpace(name, nameof(name));
            InternalContract.RequireValidated(tenant, nameof(tenant));
            InternalContract.Require(level != RunTimeLevelEnum.None, $"{nameof(level)} ({level}) must be set to something else than {RunTimeLevelEnum.None}");

            Setup.Name = name;
            Setup.Tenant = tenant;
            Setup.RunTimeLevel = level;

            Setup.ThreadHandler = ThreadHelper.RecommendedForRuntime;
            Setup.SynchronousFastLogger = LogHelper.RecommendedSyncLoggerForRuntime;
            switch (level)
            {
                case RunTimeLevelEnum.None:
                case RunTimeLevelEnum.Development:
                    Setup.LogSeverityLevelThreshold = LogSeverityLevel.Verbose;
                    break;
                case RunTimeLevelEnum.Test:
                    Setup.LogSeverityLevelThreshold = LogSeverityLevel.Information;
                    break;
                case RunTimeLevelEnum.ProductionSimulation:
                case RunTimeLevelEnum.Production:
                    Setup.LogSeverityLevelThreshold = LogSeverityLevel.Warning;
                    break;
                default:
                    Setup.LogSeverityLevelThreshold = LogSeverityLevel.Verbose;
                    InternalContract.Fail($"Parameter {nameof(level)} had an unexpected value ({level})");
                    break;
            }
        }

        /// <summary>
        /// Validate the values in <see cref="Setup"/> has been properly set.
        /// </summary>
        /// <remarks>This should be used initially for any Fulcrum library that depends on these settings.</remarks>
        public static void Validate()
        {
            try
            {
                Setup.Validate(null, $"{nameof(FulcrumApplication)}.{nameof(Setup)}");
            }
            catch (Exception e)
            {
                throw new FulcrumContractException($"{e.Message} Indicates that {nameof(FulcrumApplication)} has not been initialized properly at application startup. Please use the method {nameof(Initialize)} or a helper from {nameof(FulcrumApplicationHelper)}.");
            }
        }

        /// <summary>
        /// The context value provider that will be used all over the application.
        /// </summary>
        public static NexusContext Context { get; } = new NexusContext(new AsyncLocalContextValueProvider());

        /// <summary>
        /// Checks if the run time environment is in development stage.
        /// </summary>
        public static bool IsInDevelopment => Setup.RunTimeLevel == RunTimeLevelEnum.Development;

        /// <summary>
        /// Checks if the run time environment is in test stage.
        /// </summary>
        public static bool IsInTest => Setup.RunTimeLevel == RunTimeLevelEnum.Test;

        /// <summary>
        /// Checks if the run time environment is in production simulation stage.
        /// </summary>
        public static bool IsInProductionSimulation => Setup.RunTimeLevel == RunTimeLevelEnum.ProductionSimulation;

        /// <summary>
        /// Checks if the run time environment is in production stage.
        /// </summary>
        public static bool IsInProduction => Setup.RunTimeLevel == RunTimeLevelEnum.Production;

        /// <summary>
        /// Checks if the run time environment is in production stage or a production simulation stage
        /// </summary>
        public static bool IsInProductionOrProductionSimulation => IsInProduction || IsInProductionSimulation;

        /// <summary>
        /// If we are in production, this method does nothing. Otherwise it calls <see cref="Validate"/>.
        /// </summary>
        public static void ValidateButNotInProduction()
        {
            if (IsInProductionOrProductionSimulation) return;
            Validate();
        }

        /// <summary>
        /// Generates a representative string for logging purposes.
        /// </summary>
        /// <returns></returns>
        public static string ToLogString()
        {
            return $"App name: {Setup.Name} | App tenant: {Setup.Tenant} | Run time level: {Setup.RunTimeLevel}";
        }
    }
}
