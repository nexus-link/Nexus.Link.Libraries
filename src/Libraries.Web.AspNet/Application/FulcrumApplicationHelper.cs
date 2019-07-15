using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Threads;
using Microsoft.Extensions.Configuration;
#if NETCOREAPP
#else
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Application
{
  /// <summary>
    /// Help class to setup your application
    /// </summary>
    public static class FulcrumApplicationHelper
    {
        /// <summary>
        /// Sets the recommended application setup for a Web Api.
        /// </summary>
        /// <param name="name">The name of the application.</param>
        /// <param name="tenant">The tenant that the application itself runs in.</param>
        /// <param name="level">The run time level for the application itself.</param>
        public static void WebBasicSetup(string name, Tenant tenant, RunTimeLevelEnum level)
        {
            FulcrumApplication.Initialize(name, tenant, level);
            FulcrumApplication.Setup.ThreadHandler = ThreadHelper.RecommendedForRuntime;
            FulcrumApplication.Setup.SynchronousFastLogger = LogHelper.RecommendedSyncLoggerForRuntime;
            FulcrumApplication.Setup.FallbackLogger = LogHelper.RecommendedFallbackLoggerForRuntime;
            FulcrumApplication.AppSettings = new AppSettings(new ConfigurationManagerAppSettings());
        }

        /// <summary>
        /// Sets the recommended application setup for a Web Api.
        /// </summary>
        /// <param name="configuration">How to get app settings for <see cref="ApplicationSetup.Name"/>, 
        /// <see cref="ApplicationSetup.Tenant"/>, <see cref="ApplicationSetup.RunTimeLevel"/></param>
        /// <remarks>
        /// The settings that must exists in the <paramref name="configuration"/> ApplicationName, Organization, Environment and RunTimeLevel.
        /// </remarks>
        public static void WebBasicSetup(IConfiguration configuration)
        {
            InternalContract.RequireNotNull(configuration, nameof(configuration));
            var appSettings = new AppSettings(configuration);
            var name = appSettings.GetString("ApplicationName", true);
            var tenant = appSettings.GetTenant("Organization", "Environment", true);
            var runTimeLevel = appSettings.GetEnum<RunTimeLevelEnum>("RunTimeLevel", true);
            WebBasicSetup(name, tenant, runTimeLevel);
            FulcrumApplication.AppSettings = new AppSettings(configuration);
        }

        /// <summary>
        /// Sets the recommended application setup for a Web Api.
        /// </summary>
        /// <param name="appSettingGetter">How to get app settings for <see cref="ApplicationSetup.Name"/>, 
        /// <see cref="ApplicationSetup.Tenant"/>, <see cref="ApplicationSetup.RunTimeLevel"/></param>
        /// <remarks>
        /// If you want to use <see cref="ConfigurationManager"/> for retreiving app settings, you can use <see cref="ConfigurationManagerAppSettings"/>
        ///  as the <paramref name="appSettingGetter"/>.
        /// The settings that must exists int the <paramref name="appSettingGetter"/> ApplicationName, Organization, Environment and RunTimeLevel.
        /// </remarks>
        public static void WebBasicSetup(IAppSettingGetter appSettingGetter)
        {
            InternalContract.RequireNotNull(appSettingGetter, nameof(appSettingGetter));
            var appSettings = new AppSettings(appSettingGetter);
            var name = appSettings.GetString("ApplicationName", true);
            var tenant = appSettings.GetTenant("Organization", "Environment", true);
            var runTimeLevel = appSettings.GetEnum<RunTimeLevelEnum>("RunTimeLevel", true);
            WebBasicSetup(name, tenant, runTimeLevel);
            FulcrumApplication.AppSettings = new AppSettings(appSettingGetter);
        }
    }
}