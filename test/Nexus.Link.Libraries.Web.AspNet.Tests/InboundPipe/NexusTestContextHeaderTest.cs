using System.Net;
using System.Net.Http;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.Pipe;

#if NETCOREAPP
#else
using Microsoft.Owin.Testing;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe
{
    [TestClass]
    public class NexusTestContextHeaderTest
    {
        private HttpClient _httpClient;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(NexusTestContextHeaderTest).FullName);

            TestStartup.TranslatorService = new Mock<ITranslatorService>().Object;
            TestStartup.GetTranslatorClientName = () => Foo.ConsumerName;
        }

        [TestMethod]
        public async Task Header_Is_Setup_On_Context()
        {

#if NETCOREAPP
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();
#else
            _httpClient = TestServer.Create<TestStartup>().HttpClient;
#endif

            const string headerValue = "v1; test-id: abc-123";

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/CurrentNexusTestContextValue");
            request.Headers.Add(Constants.NexusTestContextHeaderName, headerValue);

            var response = await _httpClient.SendAsync(request);
            var resultString = await response.Content.ReadAsStringAsync();
            var result = JToken.Parse(resultString).Value<string>();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, result);
            Assert.AreEqual(headerValue, result);
        }
    }
}