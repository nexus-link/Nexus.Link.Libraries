#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.RespondAsyncFilter
{
    public class RespondAsyncFilterStartup
    {
        public static IRespondAsyncFilterSupport RespondAsyncFilterSupport { get; set; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            var respondAsyncFilter = new Pipe.Inbound.RespondAsyncFilter(RespondAsyncFilterSupport);
            var mvc = services.AddMvc(opts => { opts.Filters.Add(respondAsyncFilter); });
            mvc.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }
    }
}
#endif