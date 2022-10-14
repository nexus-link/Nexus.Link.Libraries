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
using System;
using Microsoft.Extensions.Primitives;

namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
    [TestClass]
    public class HttpSenderTest
    {
        private Mock<IHttpClient> _httpClientMock;
        private HttpRequestMessage _actualRequestMessage;
        private string _actualRequestContent;
        private string _expectedResponseContent;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(HttpSenderTest).FullName);
            _expectedResponseContent = Guid.NewGuid().ToString();
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage m, CancellationToken ct) =>
                {
                    _actualRequestMessage = m;
                    _actualRequestContent = null;
                    if (m.Content != null)
                    {
                        m.Content.LoadIntoBufferAsync().Wait(ct);
                        _actualRequestContent = m.Content.ReadAsStringAsync().Result;
                    }
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_expectedResponseContent)
                });
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
            var actualContentAsObject = JsonConvert.DeserializeObject<TestType>(_actualRequestContent);

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
                    _actualRequestContent = null;
                    if (m.Content != null)
                    {
                        m.Content.LoadIntoBufferAsync().Wait(ct);
                        _actualRequestContent = m.Content.ReadAsStringAsync().Result;
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

        [TestMethod]
        public async Task Redirect()
        {
            // Arrange
            const string baseUri = "http://example.se/";
            const string redirectUrl = "http://redirect.example.se/";
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri == redirectUrl), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage rm, CancellationToken ct) =>
                {
                    _actualRequestMessage = rm;
                    _actualRequestContent = null;
                    if (rm.Content != null)
                    {
                        rm.Content.LoadIntoBufferAsync().Wait(ct);
                        _actualRequestContent = rm.Content.ReadAsStringAsync().Result;
                    }
                })
                .ReturnsAsync((HttpRequestMessage rm, CancellationToken ct) => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = rm,
                    Content = new StringContent(JsonConvert.SerializeObject(_expectedResponseContent))
                });
            _httpClientMock.Setup(s => s.SendAsync(It.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri == baseUri), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpRequestMessage rm, CancellationToken ct) =>
                {
                    var r = new HttpResponseMessage(HttpStatusCode.Redirect)
                    {
                        RequestMessage = rm
                    };
                    r.Headers.TryAddWithoutValidation("Location", redirectUrl);
                    return r;
                });
            var sender = new HttpSender(baseUri)
            {
                HttpClient = _httpClientMock.Object,
                OptionFollowRedirectsToDepth = 1
            };

            // Act
            var result = await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", "body");

            // Assert
            _actualRequestMessage.RequestUri.OriginalString.ShouldBe(redirectUrl);
            result.Body.ShouldBe(_expectedResponseContent);
        }

        [TestMethod]
        public async Task CircularRedirect()
        {
            // Arrange
            const string baseUri = "http://example.se/";
            const string redirectUrl = baseUri;
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri == redirectUrl), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage rm, CancellationToken ct) =>
                {
                    _actualRequestMessage = rm;
                    _actualRequestContent = null;
                    if (rm.Content != null)
                    {
                        rm.Content.LoadIntoBufferAsync().Wait(ct);
                        _actualRequestContent = rm.Content.ReadAsStringAsync().Result;
                    }
                })
                .ReturnsAsync((HttpRequestMessage rm, CancellationToken ct) => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = rm,
                    Content = new StringContent(JsonConvert.SerializeObject(_expectedResponseContent))
                });
            _httpClientMock.Setup(s => s.SendAsync(It.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri == baseUri), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpRequestMessage rm, CancellationToken ct) =>
                {
                    var r = new HttpResponseMessage(HttpStatusCode.Redirect)
                    {
                        RequestMessage = rm
                    };
                    r.Headers.TryAddWithoutValidation("Location", redirectUrl);
                    return r;
                });
            var sender = new HttpSender(baseUri)
            {
                HttpClient = _httpClientMock.Object,
                OptionFollowRedirectsToDepth = 1
            };

            // Act
            var result = await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", "body")
                .ShouldThrowAsync<FulcrumResourceException>();
        }

        [DataRow(null)]
        [DataRow("")]
        [DataTestMethod]
        public async Task RedirectWithBadLocationHeader(string locationHeaderValue)
        {
            // Arrange
            const string baseUri = "http://example.se/";
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri == baseUri), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpRequestMessage rm, CancellationToken ct) =>
                {
                    var r = new HttpResponseMessage(HttpStatusCode.Redirect)
                    {
                        RequestMessage = rm
                    };
                    if (locationHeaderValue == null) return r;
                    r.Headers.TryAddWithoutValidation("Location", locationHeaderValue);
                    return r;
                });
            var sender = new HttpSender(baseUri)
            {
                HttpClient = _httpClientMock.Object,
                OptionFollowRedirectsToDepth = 1
            };

            // Act & Assert
            await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", "body")
                .ShouldThrowAsync<FulcrumResourceException>();
        }

        [DataRow(null)]
        [DataRow("")]
        [DataTestMethod]
        public async Task RedirectWithNoDepth(string locationHeaderValue)
        {
            // Arrange
            const string baseUri = "http://example.se/";
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri == baseUri), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpRequestMessage rm, CancellationToken ct) =>
                {
                    var r = new HttpResponseMessage(HttpStatusCode.Redirect)
                    {
                        RequestMessage = rm
                    };
                    if (locationHeaderValue == null) return r;
                    r.Headers.TryAddWithoutValidation("Location", locationHeaderValue);
                    return r;
                });
            var sender = new HttpSender(baseUri)
            {
                HttpClient = _httpClientMock.Object
            };

            // Act & Assert
            await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", "body")
                .ShouldThrowAsync<FulcrumResourceException>();
        }

        [TestMethod]
        public async Task RedirectWithBadLocationHeader()
        {
            // Arrange
            const string baseUri = "http://example.se/";
            var locationHeaderValues = new[] {"http://a.example.se/", "http://a.example.se/"};
            _httpClientMock = new Mock<IHttpClient>();
            _httpClientMock.Setup(s => s.SendAsync(It.Is<HttpRequestMessage>(rm => rm.RequestUri.AbsoluteUri == baseUri), It.IsAny<CancellationToken>()))
                .ReturnsAsync((HttpRequestMessage rm, CancellationToken ct) =>
                {
                    var r = new HttpResponseMessage(HttpStatusCode.Redirect)
                    {
                        RequestMessage = rm
                    };
                    foreach (var locationHeaderValue in locationHeaderValues)
                    {
                        r.Headers.TryAddWithoutValidation("Location", locationHeaderValue);
                    }
                    return r;
                });
            var sender = new HttpSender(baseUri)
            {
                HttpClient = _httpClientMock.Object,
                OptionFollowRedirectsToDepth = 1
            };

            // Act & Assert
            await sender.SendRequestAsync<string, string>(HttpMethod.Post, "", "body")
                .ShouldThrowAsync<FulcrumNotImplementedException>();
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