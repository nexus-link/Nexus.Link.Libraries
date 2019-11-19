#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support
{
    public class TestStartup
    {
        public static TranslatorFactory TranslatorFactory { get; set; }
        public virtual void ConfigureServices(IServiceCollection services)
        {
            var valueTranslatorFilter = new ValueTranslatorFilter();
            services.AddMvc(opts => { opts.Filters.Add(valueTranslatorFilter); });
            valueTranslatorFilter.TranslatorFactory = TranslatorFactory;
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
#endif