﻿using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.Libraries.Web.Tests.Error
{
    [TestClass]
    public class ExceptionConverterTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ExceptionConverterTests).FullName);
        }

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
            Assert.AreEqual(fulcrumException.TechnicalMessage, result.TechnicalMessage);
        }

        [TestMethod]
        public async Task ConvertNull()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = null
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNotNull(result);
            Assert.AreEqual(FulcrumContractException.ExceptionType, result.Type);
        }

        [TestMethod]
        public async Task ConvertEmpty()
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("", Encoding.UTF8)
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNotNull(result);
            Assert.AreEqual(FulcrumContractException.ExceptionType, result.Type);
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
            Assert.IsNotNull(result);
            Assert.AreEqual(FulcrumContractException.ExceptionType, result.Type);
        }

        [TestMethod]
        public async Task Convert404()
        {
            var content = "Not result FulcrumError";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(content, Encoding.UTF8)
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNotNull(result);
            Assert.AreEqual(FulcrumContractException.ExceptionType, result.Type);
        }

        [TestMethod]
        public async Task Convert402()
        {
            var content = "Accepted FulcrumError";
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(content, Encoding.UTF8)
            };
            var result = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            Assert.IsNotNull(result);
            Assert.AreEqual(FulcrumAcceptedException.ExceptionType, result.Type);
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
            Assert.IsNotNull(result);
            Assert.AreEqual(FulcrumContractException.ExceptionType, result.Type);
            var contentAfter = await responseMessage.Content.ReadAsStringAsync();
            Assert.AreEqual(content, contentAfter);
        }
    }
}
