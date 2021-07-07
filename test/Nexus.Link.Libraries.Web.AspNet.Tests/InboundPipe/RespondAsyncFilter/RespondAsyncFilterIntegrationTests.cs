#if NETCOREAPP
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Logic;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;
using Nexus.Link.Libraries.Web.AspNet.Queue;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.RespondAsyncFilter
{
    [TestClass]
    public class RespondAsyncFilterIntegrationTests
    {
        private HttpClient _httpClient;
        private Mock<IStoppableQueue<RequestData>> _queueMock;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(RespondAsyncFilterIntegrationTests).FullName);
            _queueMock = new Mock<IStoppableQueue<RequestData>>();

            RespondAsyncFilterStartup.RespondAsyncFilterSupport = new DefaultRespondAsyncFilterSupport(_queueMock.Object, new DefaultRequestExecutor(), new ResponseHandlerInMemory("a/{0}/b"));
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();
        }

        [TestMethod]
        public async Task Given_NoClientPreferenceAndMethodIsIndifferent_Gives_OK()
        {
            var response = await _httpClient.GetAsync($"/api/Foos/{Foo.ConsumerId1}");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Given_NoClientPreferAsyncAndMethodIndifferent_Gives_Accepted()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/Foos/{Foo.ConsumerId1}");
            _queueMock
                .Setup(q => q.EnqueueAsync(It.IsAny<RequestData>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            request.Headers.Add(Constants.PreferHeaderName, Constants.PreferRespondAsyncHeaderValue);

            //Act
            try
            {
                var response = await _httpClient.SendAsync(request);
                Assert.Fail($"Expected an exception of type {nameof(FulcrumAcceptedException)}.");
            }
            catch (FulcrumAcceptedException)
            {

            }
        }
    }
}
#endif