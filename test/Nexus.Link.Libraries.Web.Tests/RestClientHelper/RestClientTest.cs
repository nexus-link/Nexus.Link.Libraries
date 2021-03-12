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
    public class RestClientTest : TestBase
    {
        private HttpSender _httpSender;
        private HttpRequestMessage _actualRequestMessage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(RestClientTest).FullName);
            HttpClientMock = new Mock<IHttpClient>();
            _httpSender = new HttpSender("http://example.se") {HttpClient = HttpClientMock.Object};
        }

        protected void PrepareSendAsync()
        {
            HttpClientMock.Setup(s => s.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Callback((HttpRequestMessage m, CancellationToken ct) =>
                {
                    _actualRequestMessage = m;
                })
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
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
            PrepareSendAsync();
            var sender = new HttpSender(baseUri) { HttpClient = HttpClientMock.Object };

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
            var sender = new HttpSender(baseUrl) { HttpClient = HttpClientMock.Object };
            await Assert.ThrowsExceptionAsync<FulcrumContractException>(() =>
                sender.SendRequestAsync(HttpMethod.Get, "relativeUrl"));
        }

        [TestMethod]
        public void StringConstructor()
        {
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
        }

        [TestMethod]
        public async Task PostNoResponseContentWithNullHttpClient()
        {
            _httpSender.HttpClient= new HttpClientWrapper(null);
            var client = new RestClient(_httpSender);
            var person = new Person();
            await client.PostNoResponseContentAsync("", person);
        }

        [TestMethod]
        public async Task PostWithNullHttpClient()
        {
            _httpSender.HttpClient= new HttpClientWrapper(null);
            var client = new RestClient(_httpSender);
            var person = new Person();
            var id = await client.PostAsync<string, Person>("", person);
            Assert.IsNull(id);
        }

        [TestMethod]
        public async Task GetWithNullHttpClient()
        {
            _httpSender.HttpClient= new HttpClientWrapper(null);
            var client = new RestClient(_httpSender);
            var person = await client.GetAsync<Person>("");
            Assert.IsNull(person);
        }

        [TestMethod]
        public async Task PutWithNullHttpClient()
        {
            _httpSender.HttpClient= new HttpClientWrapper(null);
            var client = new RestClient(_httpSender);
            var person = new Person();
            var personOut = await client.PutAsync<Person, Person>("1", person);
            Assert.IsNull(personOut);
        }

        [TestMethod]
        public async Task DeleteWithNullHttpClient()
        {
            _httpSender.HttpClient= new HttpClientWrapper(null);
            var client = new RestClient(_httpSender);
            await client.DeleteAsync("1");
        }

        #region POST

        [TestMethod]
        public async Task PostNormal()
        {
            var person = new Person { GivenName = "GivenName", Surname = "Surname" };
            PrepareMockPost(person);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            var result = await client.PostAndReturnCreatedObjectAsync("Persons", person);
            Assert.IsNotNull(result);
            AssertAreEqual(person, result);
            HttpClientMock.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpOperationException))]
        public async Task PostNotFound()
        {
            var person = new Person { GivenName = "GivenName", Surname = "Surname" };
            var content = "Resource could not be found, 307EEC28-22DE-4BE3-8803-0AB5BE9DEBD8";
            PrepareMockNotFound(HttpMethod.Post, content);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            try
            {
                await client.PostAndReturnCreatedObjectAsync("Persons", person);
                Assert.Fail("Expected an exception.");
            }
            catch (HttpOperationException e)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, e.Response.StatusCode);
                Assert.IsTrue(e.Response.Content.Contains(content));
                HttpClientMock.VerifyAll();
                throw;
            }
        }
        #endregion

        #region GET

        [TestMethod]
        public async Task GetNormal()
        {
            var person = new Person { GivenName = "GivenName", Surname = "Surname" };
            PrepareMockGet(person);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            var result = await client.GetAsync<Person>("Persons/23");
            Assert.IsNotNull(result);
            AssertAreEqual(person, result);
            HttpClientMock.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpOperationException))]
        public async Task GetNotFound()
        {
            var content = "Resource could not be found, 307EEC28-22DE-4BE3-8803-0AB5BE9DEBD8";
            PrepareMockNotFound(HttpMethod.Get, content);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            try
            {
                await client.GetAsync<Person>("Persons/23");
                Assert.Fail("Expected an exception.");
            }
            catch (HttpOperationException e)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, e.Response.StatusCode);
                Assert.IsTrue(e.Response.Content.Contains(content));
                HttpClientMock.VerifyAll();
                throw;
            }
        }

        #endregion

        #region PUT

        [TestMethod]
        public async Task PutNormal()
        {
            var person = new Person { GivenName = "GivenName", Surname = "Surname" };
            PrepareMockPut(person);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            var result = await client.PutAndReturnUpdatedObjectAsync("Persons/23", person);
            Assert.IsNotNull(result);
            AssertAreEqual(person, result);
            HttpClientMock.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpOperationException))]
        public async Task PutNotFound()
        {
            var person = new Person { GivenName = "GivenName", Surname = "Surname" };
            var content = "Resource could not be found, 307EEC28-22DE-4BE3-8803-0AB5BE9DEBD8";
            PrepareMockNotFound(HttpMethod.Put, content);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            try
            {
                await client.PutAndReturnUpdatedObjectAsync("Persons/23", person);
                Assert.Fail("Expected an exception.");
            }
            catch (HttpOperationException e)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, e.Response.StatusCode);
                Assert.IsTrue(e.Response.Content.Contains(content));
                HttpClientMock.VerifyAll();
                throw;
            }
        }
        #endregion

        #region DELETE

        [TestMethod]
        public async Task DeleteNormal()
        {
            var person = new Person { GivenName = "GivenName", Surname = "Surname" };
            PrepareMockDelete(person);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            await client.DeleteAsync("Persons/23");
            HttpClientMock.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpOperationException))]
        public async Task DeleteNotFound()
        {
            var content = "Resource could not be found, 307EEC28-22DE-4BE3-8803-0AB5BE9DEBD8";
            PrepareMockNotFound(HttpMethod.Delete, content);
            var client = new RestClient(_httpSender);
            Assert.IsNotNull(client);
            try
            {
                await client.DeleteAsync("Persons/23");
                Assert.Fail("Expected an exception.");
            }
            catch (HttpOperationException e)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, e.Response.StatusCode);
                Assert.IsTrue(e.Response.Content.Contains(content));
                HttpClientMock.VerifyAll();
                throw;
            }
        }
        #endregion

        [TestMethod]
        public async Task TestDateParsingWithPoco()
        {
            const string dateTime1 = "2018-10-15T18:36:00+02:00";
            const string dateTime2 = "2018-10-15T12:23:27Z";

            // When using a poco, date parsing handling should be safe
            var input = new DateParsingPoco { DateTime1 = dateTime1, DateTime2 = dateTime2 };
            PrepareMockPost(input);

            var client = new RestClient(_httpSender);
            var result = await client.PostAndReturnCreatedObjectAsync("path", input);
            Assert.AreEqual(dateTime1, result.DateTime1);
            Assert.AreEqual(dateTime2, result.DateTime2);
        }

        [TestMethod]
        public async Task TestDateParsingWithJObject()
        {
            const string dateTime1 = "2018-10-15T18:36:00+02:00";
            const string dateTime2 = "2018-10-15T12:23:27Z";

            // When using JObject instead of explicit poco, Newtonsoft will try to find dates from strings.
            var input = JObject.FromObject(new { DateTime1 = dateTime1, DateTime2 = dateTime2 });
            PrepareMockPost(input);

            // Per default, the RestClient will try to parse dates, so our values will get scrambled a bit
            // This is our desired default value, so make sure to fail if it doesn't auto parse dates
            var httpSender = _httpSender;
            var client = new RestClient(httpSender);
            var result = await client.PostAndReturnCreatedObjectAsync("path", input);
            Assert.AreNotEqual(dateTime1, result.Value<string>("DateTime1"));
            Assert.AreNotEqual(dateTime2, result.Value<string>("DateTime2"));

            // Make sure we have the possibility to change behaviour to not auto parse dates
            httpSender.DeserializationSettings.DateParseHandling = DateParseHandling.None;
            PrepareMockPost(input);
            result = await client.PostAndReturnCreatedObjectAsync("path", input);
            Assert.AreEqual(dateTime1, result.Value<string>("DateTime1"));
            Assert.AreEqual(dateTime2, result.Value<string>("DateTime2"));
        }
    }

    internal class DateParsingPoco
    {
        public string DateTime1 { get; set; }
        public string DateTime2 { get; set; }
    }
}
