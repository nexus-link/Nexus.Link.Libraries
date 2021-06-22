

#if NETCOREAPP
using Microsoft.AspNetCore.TestHost;
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.RespondAsyncFilter
{
    public class CustomWebApplicationFactory : WebApplicationFactory<RespondAsyncFilterStartup>
    {
        // https://holsson.wordpress.com/2018/06/25/integration-testing-asp-net-core-2-1/
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return
                WebHost
                    .CreateDefaultBuilder(Array.Empty<string>())
                    .UseStartup<RespondAsyncFilterStartup>();
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
