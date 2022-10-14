using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.RestClientHelper;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Shouldly;
using System.Linq;

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
                .Callback((HttpRequestMessage m, CancellationToken ct) =>
                {
                    _actualRequestMessage = m;
                    _actualContent = null;
                    if (m.Content != null)
                    {
                        m.Content.LoadIntoBufferAsync().Wait(ct);
                        _actualContent = m.Content.ReadAsStringAsync().Result;
                    }
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
            var contentAsJToken = JToken.Parse(contentAsJson);
            var sender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };

            // Act
            var response = await sender.SendRequestAsync(HttpMethod.Post, "", contentAsJToken);
            var actualContentAsObject = JsonConvert.DeserializeObject<TestType>(_actualContent);

            // Assert
            Assert.AreEqual(contentAsObject.A, actualContentAsObject.A);
            Assert.AreEqual(contentAsObject.B, actualContentAsObject.B);
        }

        [TestMethod]
        public async Task PostResponseWithNoContent()
        {
            // Arrange
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage m, CancellationToken ct) =>
                {
                    _actualRequestMessage = m;
                    _actualContent = null;
                    if (m.Content != null)
                    {
                        m.Content.LoadIntoBufferAsync().Wait(ct);
                        _actualContent = m.Content.ReadAsStringAsync().Result;
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));
            const string baseUri = "http://example.se/";
            var content = "content";
            var sender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };

            // Act
            var response = await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", content);

            // Assert
            Assert.IsNull(response.Body);
        }

        [TestMethod]
        public async Task RedirectDoesNotFailEarly()
        {
            // Arrange
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
               
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Redirect));
            const string baseUri = "http://example.se/";
            var content = "content";
            var sender = new HttpSender(baseUri) { HttpClient = _httpClientMock.Object };

            // Act
            var response = await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", content);

            // Assert
            response.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        }

        [DataRow(":method")]
        [DataRow("Content-Type")]
        [DataTestMethod]
        public async Task BadHeaderNames(string headerName)
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new HttpSenderForTest(baseUri) { HttpClient = _httpClientMock.Object };
            var headers = new Dictionary<string, List<string>> { { headerName, new List<string> { "value" } } };
            var request = await baseHttpSender.CreateRequestAsync(HttpMethod.Post, "relative",
                headers);

        }

        [DataRow("application/xml")]
        [DataRow("random text")]
        [DataTestMethod]
        public async Task CustomAcceptHeader(string expectedAcceptHeader)
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new HttpSenderForTest(baseUri) { HttpClient = _httpClientMock.Object };
            var headers = new Dictionary<string, List<string>> { { "Accept", new List<string> { expectedAcceptHeader } } };
            var request = await baseHttpSender.CreateRequestAsync(HttpMethod.Post, "relative",
                headers);

            var found = request.Headers.TryGetValues("Accept", out var accept);
            found.ShouldBeTrue();
            var acceptArray = accept.ToArray();
            acceptArray.Length.ShouldBe(1);
            acceptArray[0].ShouldBe(expectedAcceptHeader);
        }

        [TestMethod]
        public async Task CanAccessCredentialsAtSendAsync()
        {
            // Arrange
            const string baseUri = "http://example.se";
            var credentials = new BasicAuthenticationCredentials { UserName = "foo" };
            var baseHttpSender = new HttpSenderForTest(baseUri, credentials) { HttpClient = _httpClientMock.Object };
            var request = new HttpRequestMessage(HttpMethod.Get, baseUri);

            // Act
            await baseHttpSender.Credentials.ProcessHttpRequestAsync(request, default);
            await baseHttpSender.SendAsync(request);

            // Assert
            _actualRequestMessage.Headers.Authorization.ShouldNotBeNull();
        }
    }

    public class TestType
    {
        public string A { get; set; }
        public int B { get; set; }
    }

    public class HttpSenderForTest : HttpSender
    {
        public HttpSenderForTest(string baseUri) : base(baseUri)
        {
        }

        public HttpSenderForTest(string baseUri, ServiceClientCredentials credentials) : base(baseUri, credentials)
        {
        }

        public new Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string relativeUrl,
            Dictionary<string, List<string>> customHeaders)
        {
            return base.CreateRequestAsync(method, relativeUrl, customHeaders);
        }
    }
}