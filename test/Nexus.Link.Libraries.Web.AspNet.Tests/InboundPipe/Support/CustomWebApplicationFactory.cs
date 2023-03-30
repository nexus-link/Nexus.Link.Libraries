

#if NETCOREAPP
using Microsoft.AspNetCore.TestHost;
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TestStartup>
    {
        //public TimeSpan? KeepAliveTimeout { get; set; }

        // https://holsson.wordpress.com/2018/06/25/integration-testing-asp-net-core-2-1/
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return
                WebHost
                    .CreateDefaultBuilder(Array.Empty<string>())
                    //.ConfigureServices((context, services) =>
                    //{
                    //    if (KeepAliveTimeout.HasValue)
                    //    {
                    //        services.Configure<KestrelServerOptions>(options =>
                    //            options.Limits.KeepAliveTimeout = KeepAliveTimeout.Value);
                    //    }
                    //})
                    .UseStartup<TestStartup>();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseSolutionRelativeContentRoot("test/Nexus.Link.Libraries.Web.AspNet.Tests");
            builder.ConfigureAppConfiguration(config =>
            {
                var integrationConfig = new ConfigurationBuilder()
                    .AddJsonFile("integrationsettings.json")
                    .Build();

                config.AddConfiguration(integrationConfig);
            });


            builder.ConfigureServices(services =>
            {
            });
        }
    }
}
#endif
