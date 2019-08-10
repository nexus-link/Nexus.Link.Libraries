#if NETCOREAPP
using Microsoft.Extensions.Configuration;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.AspNet.Startup
{
    /// <summary>
    /// Helper class for the Program.cs file.
    /// </summary>
    public static class ProgramHelper
    {
        /// <summary>
        /// Add .ConfigureAppConfiguration(ProgramHelper.AddConfiguration) before the .UseStartup call.
        /// </summary>
        public static void AddConfiguration(IConfigurationBuilder builder)
        {
            var configuration = builder.Build();
            var configurationSection = configuration.GetSection("FulcrumApplication");
            FulcrumAssert.IsNotNull(configurationSection);
            Application.FulcrumApplicationHelper.WebBasicSetup(configurationSection);
        }

        /// <summary>
        /// This is aimed at to be used by other AddConfiguration() methods
        /// </summary>
        /// <param name="configuration"></param>
        public static void AddConfiguration(IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection("FulcrumApplication");
            FulcrumAssert.IsNotNull(configurationSection);
            Application.FulcrumApplicationHelper.WebBasicSetup(configurationSection);
        }
    }
}
#endif