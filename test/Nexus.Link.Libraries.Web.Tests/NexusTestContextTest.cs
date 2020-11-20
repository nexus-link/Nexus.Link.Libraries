using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Tests.Support;

namespace Nexus.Link.Libraries.Web.Tests
{
    [TestClass]
    public class NexusTestContextTest
    {
        [TestInitialize]
        public void RunForEveryTestCase()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(NexusTestContextTest).FullName);
        }

        /// <summary>
        /// Given that FulcrumApplication.Context.NexusTestContext is setup, that value should be propagated as the header <see cref="Constants.NexusTestHeaderName"/>
        /// </summary>
        [TestMethod]
        public async Task Header_Is_Propagated()
        {
            const string headerValue = "v1; test-id: abc-123";
            FulcrumApplication.Context.NexusTestContext = headerValue;

            var handler = new PropagateNexusTestHeaderForTest();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            await handler.SendAsync(request);

            Assert.IsTrue(request.Headers.TryGetValues(Constants.NexusTestHeaderName, out var headerValues), $"Expected {Constants.NexusTestHeaderName} header to be present");
            Assert.AreEqual(headerValue, headerValues.First());
        }

        [TestMethod]
        public async Task No_Header_In_Request()
        {
            var handler = new PropagateNexusTestHeaderForTest();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/");
            await handler.SendAsync(request);

            Assert.IsFalse(request.Headers.TryGetValues(Constants.NexusTestHeaderName, out _), $"Expected no {Constants.NexusTestHeaderName} header to be present");
        }
    }
}
