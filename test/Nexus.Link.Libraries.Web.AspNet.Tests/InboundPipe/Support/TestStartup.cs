
#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Core.Translation;
using Microsoft.AspNetCore.Mvc;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
    public class TestStartup
    {
        public static TranslatorFactory TranslatorFactory { get; set; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            var valueTranslatorFilter = new ValueTranslatorFilter();
            var mvc = services
                .AddMvc(opts => { opts.Filters.Add(valueTranslatorFilter); });
            mvc
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            ;
            valueTranslatorFilter.TranslatorFactory = TranslatorFactory;
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }

    public class InspectControllers : IApplicationFeatureProvider
    {

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature 
            feature)
        {
            var serviceAssembly = Assembly.GetEntryAssembly();
            foreach (var controller in feature.Controllers)
            {
                Console.WriteLine(controller.FullName);
            }
        }
    }
}
#endif