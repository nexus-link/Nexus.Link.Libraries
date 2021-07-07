#if NETCOREAPP
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
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
        [DataRow("POST", "http://example.com", true)]
        [DataRow("POST", "http://example.com/", false)]
        [DataRow("POST", "http://example.com/api/Persons", false)]
        [DataRow("POST", "http://example.com/api/Persons/", false)]
        [DataRow("POST", "http://example.com/api/Persons/123?test=1", false)]
        [DataRow("POST", "http://example.com/api/Persons/123/?test=1", false)]
        public async Task SerializeHttpRequestMessage(string expectedMethod, string expectedUrl, bool slashAdded)
        {
            // Arrange
            var expectedObject = expectedMethod == "GET" ? (Person) null : new Person { Name = "ExpectedName" };
            var method = new HttpMethod(expectedMethod);
            var request = new HttpRequestMessage(method, expectedUrl)
            {
                Content = expectedObject == null ? 
                    (HttpContent) new StringContent("", Encoding.UTF8)
                    : (HttpContent) new ObjectContent<Person>(expectedObject, new JsonMediaTypeFormatter(), "application/json")
            };

            // Act
            var requestData = await new RequestData().FromAsync(request);
            Assert.IsNotNull(requestData.BodyAsString);
            var actualObject = string.IsNullOrWhiteSpace(requestData.BodyAsString) ? null : JObject.Parse(requestData.BodyAsString).ToObject<Person>();

            // Assert
            if (slashAdded) expectedUrl += "/";
            Assert.AreEqual(expectedMethod, requestData.Method);
            Assert.AreEqual(expectedUrl, requestData.EncodedUrl);
            Assert.AreEqual(expectedObject?.Name, actualObject?.Name);
        }

        [DataTestMethod]
        [DataRow("GET", "http://example.com", true)]
        [DataRow("GET", "http://example.com/", false)]
        [DataRow("GET", "http://example.com/api/Persons", false)]
        [DataRow("GET", "http://example.com/api/Persons/", false)]
        [DataRow("GET", "http://example.com/api/Persons/123?test=1", false)]
        [DataRow("GET", "http://example.com/api/Persons/123/?test=1", false)]
        [DataRow("POST", "http://example.com", true)]
        [DataRow("POST", "http://example.com/", false)]
        [DataRow("POST", "http://example.com/api/Persons", false)]
        [DataRow("POST", "http://example.com/api/Persons/", false)]
        [DataRow("POST", "http://example.com/api/Persons/123?test=1", false)]
        [DataRow("POST", "http://example.com/api/Persons/123/?test=1", false)]
        public async Task SerializeHttpRequest(string expectedMethod, string expectedUrl, bool slashAdded)
        {
            // Arrange
            var expectedObject = expectedMethod == "GET" ? null : new Person { Name = "ExpectedName" };
            var expectedJObject = expectedObject == null ? null : JObject.FromObject(expectedObject);
            var request = NexusLinkMiddleware.Support.CreateRequest(expectedUrl, expectedMethod, expectedJObject);

            // Act
            var requestData = await new RequestData().FromAsync(request);
            Assert.IsNotNull(requestData.BodyAsString);
            var actualObject = string.IsNullOrWhiteSpace(requestData.BodyAsString) ? null : JObject.Parse(requestData.BodyAsString).ToObject<Person>();

            // Assert
            if (slashAdded) expectedUrl += "/";
            Assert.AreEqual(expectedMethod, requestData.Method);
            Assert.AreEqual(expectedUrl, requestData.EncodedUrl);
            Assert.AreEqual(expectedObject?.Name, actualObject?.Name);
        }

        [DataTestMethod]
        [DataRow("GET", "http://example.com", true)]
        [DataRow("GET", "http://example.com/", false)]
        [DataRow("GET", "http://example.com/api/Persons", false)]
        [DataRow("GET", "http://example.com/api/Persons/", false)]
        [DataRow("GET", "http://example.com/api/Persons/123?test=1", false)]
        [DataRow("GET", "http://example.com/api/Persons/123/?test=1", false)]
        [DataRow("POST", "http://example.com", true)]
        [DataRow("POST", "http://example.com/", false)]
        [DataRow("POST", "http://example.com/api/Persons", false)]
        [DataRow("POST", "http://example.com/api/Persons/", false)]
        [DataRow("POST", "http://example.com/api/Persons/123?test=1", false)]
        [DataRow("POST", "http://example.com/api/Persons/123/?test=1", false)]
        public async Task DeserializeRequest(string expectedMethod, string expectedUrl, bool slashAdded)
        {
            // Arrange
            var expectedObject = expectedMethod == "GET" ? null : new Person { Name = "ExpectedName" };
            var expectedJObject = expectedObject == null ? null : JObject.FromObject(expectedObject);
            var expectedRequest = NexusLinkMiddleware.Support.CreateRequest(expectedUrl, expectedMethod, expectedJObject);
            var requestData = await new RequestData().FromAsync(expectedRequest);

            // Act
            var actualRequest = requestData.ToHttpRequestMessage();
            await actualRequest.Content.LoadIntoBufferAsync();
            var actualContent = await actualRequest.Content.ReadAsStringAsync();

            // Assert
            Assert.IsNotNull(actualRequest);
            if (slashAdded) expectedUrl += "/";
            Assert.AreEqual(expectedMethod, actualRequest.Method.ToString());
            Assert.AreEqual(expectedUrl, actualRequest.RequestUri.AbsoluteUri);
            Assert.AreEqual(expectedRequest.ContentLength, actualContent.Length);
        }
    }

    public class Person
    {
        public string Name { get; set; }
    }
}
#endif