using System;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;

#if NETCOREAPP
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Nexus.Link.Libraries.Core.Application;

#pragma warning disable 618
#else
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Owin;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
    public class TestStartup
    {
        public static ITranslatorService TranslatorService { get; set; }
        public static Func<string> GetTranslatorClientName { get; set; }

#if NETCOREAPP
        public virtual void ConfigureServices(IServiceCollection services)
        {
            var valueTranslatorFilter = new ValueTranslatorFilter(TranslatorService, GetTranslatorClientName);
            var mvc = services.AddMvc(opts =>
            {
                opts.Filters.Add(valueTranslatorFilter);
                opts.EnableEndpointRouting = false;
            });
            mvc.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var keepAliveTimeout = FulcrumApplication.Context.ValueProvider.GetValue<TimeSpan?>("KeepAliveTimeout");
            if (keepAliveTimeout.HasValue)
            {
                services.Configure<KestrelServerOptions>(options => options.Limits.KeepAliveTimeout = keepAliveTimeout.Value);
                services.Configure<KestrelServerOptions>(options => options.Limits.RequestHeadersTimeout = keepAliveTimeout.Value);
            }
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseNexusSaveTestContext();
            app.UseMvc();
        }
#else
        public void Configuration(IAppBuilder app)
        {
            var httpConfiguration = new HttpConfiguration();

            httpConfiguration.Services.Replace(typeof(IExceptionHandler), new ConvertExceptionToFulcrumResponse());
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Filters.Add(new ValueTranslatorFilter(TranslatorService, GetTranslatorClientName));
            httpConfiguration.MessageHandlers.Add(new SaveNexusTestContext());
            httpConfiguration.EnsureInitialized();

            app.UseWebApi(httpConfiguration);
        }
#endif
    }

#if NETCOREAPP
    public class InspectControllers : IApplicationFeatureProvider
    {

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var serviceAssembly = Assembly.GetEntryAssembly();
            foreach (var controller in feature.Controllers)
            {
                Console.WriteLine(controller.FullName);
            }
        }
    }
#endif
}