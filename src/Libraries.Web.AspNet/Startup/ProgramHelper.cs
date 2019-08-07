#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Swashbuckle.AspNetCore.Swagger;

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