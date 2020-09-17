using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.Tests.Support.Models;

namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
    [TestClass]
    public class HttpSenderTest
    {
        private Mock<IHttpClient> _httpClientMock;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(HttpSenderTest).FullName);
            _httpClientMock = new Mock<IHttpClient>();
        }

        [TestMethod]
        public void RelativePath()
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new HttpSender(baseUri) {HttpClient = _httpClientMock.Object};
            Assert.AreEqual($"{baseUri}/", baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}/{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]
        public void QuestionMark()
        {
            const string baseUri = "http://example.se";
            var baseHttpSender = new HttpSender(baseUri) {HttpClient = _httpClientMock.Object};
            Assert.AreEqual($"{baseUri}/", baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "?a=Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}/{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }

        [TestMethod]
        public void BaseEndsInSlash()
        {
            const string baseUri = "http://example.se/";
            var baseHttpSender = new HttpSender(baseUri) {HttpClient = _httpClientMock.Object};
            Assert.AreEqual(baseUri, baseHttpSender.BaseUri?.AbsoluteUri);
            const string relativeUrl = "Test";
            var relativeHttpSender = baseHttpSender.CreateHttpSender(relativeUrl);
            Assert.AreEqual($"{baseUri}{relativeUrl}", relativeHttpSender.BaseUri?.AbsoluteUri);
        }
    }
}
