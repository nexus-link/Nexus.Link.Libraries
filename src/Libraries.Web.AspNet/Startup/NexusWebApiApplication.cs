﻿#if !NETCOREAPP
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.Libraries.Web.AspNet.Startup
{
    /// <summary>
    /// Convenience class for helping with application start from Global.asax.cs
    /// </summary>
    [Obsolete("We don't use Fundamentals for configuration for service tenants anymore. Obsolete warning since 2021-06-09.")]
    public abstract class NexusWebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// If the startup fails, we will be left with this exception, which we will show on subsequent requests
        /// </summary>
        // ReSharper disable once RedundantDefaultMemberInitializer
        private static Exception _startupError = null;

        /// <summary>
        /// Returns the <see cref="ILeverConfiguration"/> for the service tenant
        /// </summary>
        protected abstract Task<ILeverConfiguration> FetchConfigurationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Operations to do before fetching Nexus configuration
        /// </summary>
        protected abstract Task ApplicationStartBeforeFetchingNexusConfigurationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Operations to do after fetching Nexus configuration
        /// </summary>
        protected abstract Task ApplicationStartAfterFetchingNexusConfigurationAsync(ILeverConfiguration configuration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the service tenant
        /// </summary>
        protected abstract Tenant ServiceTenant { get; }

        protected virtual void Application_Start()
        {
            try
            {
                ThreadHelper.CallAsyncFromSync(async () => await ApplicationStartBeforeFetchingNexusConfigurationAsync());

                var configuration = ThreadHelper.CallAsyncFromSync(async () => await FetchConfigurationWithRetriesOnFailAsync());
                if (configuration == null)
                {
                    throw new FulcrumResourceException($"{FulcrumApplication.Setup?.Name}:" +
                                                       $" (InstanceId: {Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")})" +
                                                       $" Could not load configuration from Fundamentals" +
                                                       $" for the service tenant {ServiceTenant}");
                }

                ThreadHelper.CallAsyncFromSync(async () => await ApplicationStartAfterFetchingNexusConfigurationAsync(configuration));
            }
            catch (Exception e)
            {
                _startupError = e;
                LogHelper.FallbackSafeLog(LogSeverityLevel.Critical, "Failure in Application_Start", e);
            }
        }

        protected async Task<ILeverConfiguration> FetchConfigurationWithRetriesOnFailAsync(CancellationToken cancellationToken = default)
        {
            ILeverConfiguration configuration = null;
            var maxRetryTimeSecondsString = new ConfigurationManagerAppSettings().GetAppSetting("MaxStartupRetryTimeInSeconds") ?? "100";
            var failCount = 0;

            var watch = Stopwatch.StartNew();
            while (watch.Elapsed < TimeSpan.FromSeconds(double.Parse(maxRetryTimeSecondsString)))
            {
                try
                {
                    configuration = await FetchConfigurationAsync(cancellationToken);
                    if (configuration != null) break;
                }
                catch (Exception e)
                {
                    failCount++;
                    LogHelper.FallbackSafeLog(LogSeverityLevel.Warning,
                        $"(InstanceId: {Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")}) " +
                        $"Failed to fetch configuration for service tenant {ServiceTenant}." +
                        $" This was try number {failCount} after {watch.Elapsed.TotalSeconds} s.", e);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }

            return configuration;
        }

        protected virtual void Application_BeginRequest()
        {
            if (_startupError != null)
            {
                try
                {
                    var exception = new FulcrumResourceException($"FATAL ERROR IN STARTUP for {FulcrumApplication.Setup?.Name}" +
                                                                 $" (InstanceId: {Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")}):" +
                                                                 $" {_startupError.Message}. Check fallback logs for details.");
                    var error = ExceptionConverter.ToFulcrumError(exception, true);
                    var statusCode = ExceptionConverter.ToHttpStatusCode(error);

                    // TrySkipIisCustomErrors = true is important! Without it, IIS sets it's own response content.
                    // See https://weblog.west-wind.com/posts/2017/jun/01/bypassing-iis-error-messages-in-aspnet
                    Response.TrySkipIisCustomErrors = true;

                    Response.StatusCode = statusCode != null ? (int)statusCode : (int)HttpStatusCode.InternalServerError;
                    Response.ContentType = "application/json";
                    Response.Output.Write(JsonConvert.SerializeObject(error));
                }
                catch (Exception e)
                {
                    LogHelper.FallbackSafeLog(LogSeverityLevel.Critical, "Failure in Application_BeginRequest", e);
                    throw;
                }
            }
        }
    }
}

#endif
