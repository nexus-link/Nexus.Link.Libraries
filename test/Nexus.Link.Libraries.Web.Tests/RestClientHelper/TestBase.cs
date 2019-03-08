using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.Tests.Support.Models;

namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
    public class TestBase
    {
        protected static Mock<IHttpClient> HttpClientMock;
        protected static void PrepareMockPost<T>(T content)
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JObject.FromObject(content).ToString(Formatting.Indented), Encoding.UTF8)
            };
            PrepareMockOk(mockResponse, HttpMethod.Post);
        }

        protected static void PrepareMockGet<T>(T content)
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JObject.FromObject(content).ToString(Formatting.Indented), Encoding.UTF8)
            };
            PrepareMockOk(mockResponse, HttpMethod.Get);
        }

        protected static void PrepareMockPut<T>(T content)
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JObject.FromObject(content).ToString(Formatting.Indented), Encoding.UTF8)
            };
            PrepareMockOk(mockResponse, HttpMethod.Put);
        }

        // ReSharper disable once UnusedParameter.Local
        protected static void PrepareMockDelete<T>(T content)
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);
            PrepareMockOk(mockResponse, HttpMethod.Delete);
        }

        protected static void AssertAreEqual(Person expected, Person actual)
        {
            Assert.AreEqual(expected.GivenName, actual.GivenName);
            Assert.AreEqual(expected.Surname, actual.Surname);
        }

        private static void PrepareMockOk(HttpResponseMessage mockResponse, HttpMethod method)
        {
            HttpClientMock.Setup(mock => mock.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == method),
                It.IsAny<CancellationToken>())).ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                mockResponse.RequestMessage = request;
                return mockResponse;
            });
        }

        protected static void PrepareMockNotFound(HttpMethod method, string errorMessage)
        {
            var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8)
            };
            HttpClientMock.Setup(mock => mock.SendAsync(It.Is<HttpRequestMessage>(m => m.Method == method),
                It.IsAny<CancellationToken>())).ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                mockResponse.RequestMessage = request;
                return mockResponse;
            });
        }

        protected HttpResponseMessage CreateResponseMessage(HttpRequestMessage request, HttpStatusCode statusCode,
            string content)
        {
            return new HttpResponseMessage(statusCode)
            {
                RequestMessage = request,
                Content = content == null ? null : new StringContent(content, Encoding.UTF8)
            };
        }

        protected HttpResponseMessage CreateResponseMessage<T>(HttpRequestMessage request, T body)
        {
            return CreateResponseMessage(request, HttpStatusCode.OK, JsonConvert.SerializeObject(body));
        }

        protected HttpResponseMessage CreateResponseMessage(HttpRequestMessage request)
        {
            return CreateResponseMessage(request, HttpStatusCode.NoContent, null);
        }
    }
}
