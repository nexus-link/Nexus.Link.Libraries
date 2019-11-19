#if NETCOREAPP
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
            services
                .AddMvc(opts => { opts.Filters.Add(valueTranslatorFilter); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                // Tried to use this to avoid 404 not found
                // https://github.com/aspnet/AspNetCore/issues/8428
                .ConfigureApplicationPartManager(p =>
                {
                    var assembly = typeof(TestStartup).Assembly;
                    var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                    foreach (var part in partFactory.GetApplicationParts(assembly))
                    {
                        p.ApplicationParts.Add(part);
                    }
                });
            valueTranslatorFilter.TranslatorFactory = TranslatorFactory;
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
#endif