#if NETCOREAPP
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.NexusLinkMiddleware;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.RespondAsyncFilter
{
    [TestClass]
    public class RequestExecutorTests
    {
        private IRequestExecutor _executor;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(RequestExecutorTests).FullName);
            _executor = new DefaultRequestExecutor(new HttpClient());
        }
        
        // TODO: Complete this test.
        [TestMethod]
        public async Task ExecuteRequest()
        {
            var context = new DefaultHttpContext();
            context.SetRequest("http://example.com/");
            context.Request.Headers.Add(Constants.PreferHeaderName, Constants.PreferRespondAsyncHeaderValue);
            var requestData = await new RequestData().FromAsync(context.Request);
            var response = await _executor.ExecuteRequestAsync(requestData);
        }
    }
}
#endif