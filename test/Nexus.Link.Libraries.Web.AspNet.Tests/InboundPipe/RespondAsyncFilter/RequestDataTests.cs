#if NETCOREAPP
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.RespondAsyncFilter
{
    [TestClass]
    public class RequestDataTests
    {
        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ResponseHandlerInMemoryTests).FullName);
        }

        [DataTestMethod]
        [DataRow("GET", "http://example.com", true)]
        [DataRow("GET", "http://example.com/", false)]
        [DataRow("GET", "http://example.com/api/Persons", false)]
        [DataRow("GET", "http://example.com/api/Persons/", false)]
        [DataRow("GET", "http://example.com/api/Persons/123?test=1", false)]
        [DataRow("GET", "http://example.com/api/Persons/123/?test=1", false)]
        public async Task SerializeRequest(string expectedMethod, string expectedUrl, bool slashAdded)
        {
            // Arrange
            var expectedObject = new Person { Name = "ExpectedName" };
            var expectedJObject = JObject.FromObject(expectedObject);
            var request = NexusLinkMiddleware.Support.CreateRequest(expectedUrl, expectedMethod, expectedJObject);

            // Act
            var requestData = await new RequestData().FromAsync(request);
            Assert.IsNotNull(requestData.BodyAsString);
            var actualObject = JObject.Parse(requestData.BodyAsString).ToObject<Person>();

            // Assert
            if (slashAdded) expectedUrl += "/";
            Assert.AreEqual(expectedMethod, requestData.Method);
            Assert.AreEqual(expectedUrl, requestData.EncodedUrl);
            Assert.AreEqual(expectedObject.Name, actualObject.Name);
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }
}
#endif