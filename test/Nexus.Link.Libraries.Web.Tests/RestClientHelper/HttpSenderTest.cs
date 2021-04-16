using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.Tests.Support.Models;

namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
    [TestClass]
    public class HttpSenderTest
    {
        private Mock<IHttpClient> _httpClientMock;
        private HttpRequestMessage _actualRequestMessage;
        private string _actualContent;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(HttpSenderTest).FullName);
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback(async (HttpRequestMessage m, CancellationToken ct) =>
                {
                    await m.Content.LoadIntoBufferAsync();
                    _actualRequestMessage = m;
                    _actualContent = await m.Content.ReadAsStringAsync();
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        }

        [TestMethod]
        public void EmptyBaseUriHasCorrectUrl()
        {
            const string baseUri = "";
            var _ = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };
        }

        [TestMethod]
        public void EmptyBaseUri()
        {
            const string baseUri = "";
            var _ = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };
        }

        [TestMethod]
        public void NullBaseUri()
        {
            const string baseUri = null;
            var _ = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };
        }

        [TestMethod]
        public void WhitespaceBaseUri()
        {
            const string baseUri = "  ";
            var _ = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };
        }

        [DataTestMethod]
        [DataRow("", "https://example.com", "https://example.com")]
        [DataRow(" ", "https://example.com", "https://example.com")]
        [DataRow(null, "https://example.com", "https://example.com")]
        [DataRow("https://example.com", "", "https://example.com")]
        [DataRow("https://example.com", " ", "https://example.com")]
        [DataRow("https://example.com", "/tests", "https://example.com/tests")]
        [DataRow("https://example.com", "tests", "https://example.com/tests")]
        [DataRow("https://example.com", "?test=123", "https://example.com?test=123")]
        [DataRow("https://example.com", null, "https://example.com")]
        public async Task BaseUrlAndRelativeUrlTests(string baseUri, string relativeUrl, string expectedUrl)
        {
            // Arrange
            var sender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };

            // Act
            await sender.SendRequestAsync(HttpMethod.Get, relativeUrl);

            // Assert
            Assert.AreEqual(expectedUrl, _actualRequestMessage.RequestUri.OriginalString);
        }

        [DataTestMethod]
        [DataRow("", "tests/123")]
        [DataRow(" ", "tests/123")]
        [DataRow(null, "tests/123")]
        public async Task BaseUrlAndRelativeUrlTests_Throws(string baseUrl, string relativeUrl)
        {
            var sender = new HttpSender(baseUrl) { HttpClient = _httpClientMock.Object };
            await Assert.ThrowsExceptionAsync<FulcrumContractException>(() =>
                sender.SendRequestAsync(HttpMethod.Get, "relativeUrl"));
        }

        [TestMethod]
        public void RelativePath()
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };
            Assert.AreEqual($"{baseUri}/", baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}/{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]
        public void QuestionMark()
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };
            Assert.AreEqual($"{baseUri}/", baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "?a=Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}/{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]

        public void BaseEndsInSlash()
        {
            const string baseUri = "http://example.se/";
            var baseHttpSender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };
            Assert.AreEqual(baseUri, baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]
        public async Task JTokenAsBody()
        {
            // Arrange
            const string baseUri = "http://example.se/";
            var contentAsObject = new TestType { A = "The string", B = 113 };
            var contentAsJson = JsonConvert.SerializeObject(contentAsObject);
            var contentAsJtoken = JToken.Parse(contentAsJson);
            var sender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };

            // Act
            var response = await sender.SendRequestAsync(HttpMethod.Post, "", contentAsJtoken);
            var actualContentAsObject = JsonConvert.DeserializeObject<TestType>(_actualContent);

            // Assert
            Assert.AreEqual(contentAsObject.A, actualContentAsObject.A);
            Assert.AreEqual(contentAsObject.B, actualContentAsObject.B);
        }
    }

    public class TestType
    {
        public string A { get; set; }
        public int B { get; set; }
    }
}
