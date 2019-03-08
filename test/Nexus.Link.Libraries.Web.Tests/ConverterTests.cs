using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.Libraries.Web.Tests
{
    [TestClass]
    public class ConverterTests
    {

        [TestMethod]
        public async Task ConvertNormal()
        {
            var fulcrumException = new FulcrumServiceContractException("Test message");
            var fulcrumError = new FulcrumError();
            fulcrumError.CopyFrom(fulcrumException);
            Assert.IsNotNull(fulcrumError);
            var json = JObject.FromObject(fulcrumError);
            var content = json.ToString(Formatting.Indented);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(content, Encoding.UTF8)
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            fulcrumError = new FulcrumError();
            fulcrumError.CopyFrom(result);
            json = JObject.FromObject(fulcrumError);
            var resultAsString = json.ToString(Formatting.Indented);
            Assert.AreEqual(content, resultAsString);
        }

        [TestMethod]
        public async Task ConvertNull()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = null
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ConvertEmpty()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("", Encoding.UTF8)
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ConvertNotFulcrumError()
        {
            var content = "Not result FulcrumError";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(content, Encoding.UTF8)
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task ConvertNotFulcrumErrorAndAccessAfter()
        {
            var content = "Not result FulcrumError";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(content, Encoding.UTF8)
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNull(result);
            var contentAfter = await responseMessage.Content.ReadAsStringAsync();
            Assert.AreEqual(content, contentAfter);
        }

        [TestMethod]
        public void ConvertStandardExceptionToFulcrumError()
        {
            const string exceptionMessage = "ExceptionMessage";
            var exception = new Exception(exceptionMessage);
            var error = ExceptionConverter.ToFulcrumError(exception);
            Assert.IsNull(error);
            error = ExceptionConverter.ToFulcrumError(exception, true);
            Assert.IsNotNull(error);
            Assert.AreEqual(exceptionMessage, error.TechnicalMessage);
        }
    }
}
