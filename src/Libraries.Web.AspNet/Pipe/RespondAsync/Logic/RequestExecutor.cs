#if NETCOREAPP
namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic
{
    public class RequestExecutor : RequestExecutorBase
    {
        public RequestExecutor()
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