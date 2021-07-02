#if NETCOREAPP
namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic
{
    public class DefaultRequestExecutor : RequestExecutorBase
    {
        public DefaultRequestExecutor()
        {
#if false
            // I would like to do something like this
            var webApplicationFactory = new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Startup>();
            HttpClient = webApplicationFactory.CreateDefaultClient();
#endif
        }
    }
}
#endif