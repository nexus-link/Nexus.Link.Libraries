using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Nexus.Link.Libraries.Web.Tests.RestClientHelper;
using Nexus.Link.Libraries.Web.Tests.Support.Models;

namespace Nexus.Link.Libraries.Web.Tests.Logging
{
    [TestClass]
    public class LoggableWebApiExtensionsTest : TestBase
    {
        [TestInitialize]
        public void RunBeforeEachTestCase()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LoggableWebApiExtensionsTest).FullName);
        }

        [TestMethod]
        public async Task ToLogStringOkResponse()
        {
            var response = CreateResponseMessage(null, HttpStatusCode.NoContent, "\"This string should be ignored\"");
            var logString = await response.ToLogStringAsync();
            Assert.IsNotNull(logString);
            Assert.AreEqual(logString, "204 (NoContent)");
        }

        [TestMethod]
        public async Task ToLogStringNullContent()
        {
            var response = CreateResponseMessage(null, HttpStatusCode.NotFound, null);
            var logString = await response.ToLogStringAsync();
            Assert.IsNotNull(logString);
            Assert.AreEqual(logString, "404 (NotFound)");
        }

        [TestMethod]
        public async Task ToLogStringWithXmlResponse()
        {
            var body = "<html/>";
            var response = CreateResponseMessage(null, HttpStatusCode.NotFound, body);
            var logString = await response.ToLogStringAsync();
            Assert.IsNotNull(logString);
            Assert.AreEqual(logString, $"404 (NotFound) | Response content was not JSON: {body}");
        }

        [TestMethod]
        public async Task ToLogStringWithLongXmlResponse()
        {
            var body = "<html>This is a very long string that is more than 40 characters.<html/>";
            var response = CreateResponseMessage(null, HttpStatusCode.NotFound, body);
            var logString = await response.ToLogStringAsync();
            Assert.IsNotNull(logString);
            Assert.AreEqual(logString, $"404 (NotFound) | Response content was not JSON. Truncated: {body.Substring(0,40)}...");
        }
    }
}
