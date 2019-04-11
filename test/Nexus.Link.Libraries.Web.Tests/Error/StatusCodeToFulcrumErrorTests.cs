using System;
using System.Net;
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
    public class StatusCodeToFulcrumErrorTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(StatusCodeToFulcrumErrorTests).FullName);
        }

        [TestMethod]
        public async Task StatusCode3xx()
        {
            // Represents 3xx
            await Verify(HttpStatusCode.Ambiguous, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.MultipleChoices, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.MovedPermanently, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.Found, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.SeeOther, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.NotModified, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.UseProxy, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.TemporaryRedirect, FulcrumServiceContractException.ExceptionType);
        }

        [TestMethod]
        public async Task StatusCode4xx()
        {
            await Verify(HttpStatusCode.BadRequest, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.Unauthorized, FulcrumUnauthorizedException.ExceptionType);
            await Verify(HttpStatusCode.PaymentRequired, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.Forbidden, FulcrumForbiddenAccessException.ExceptionType);
            await Verify(HttpStatusCode.NotFound, FulcrumNotImplementedException.ExceptionType);
            await Verify(HttpStatusCode.MethodNotAllowed, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.NotAcceptable, FulcrumServiceContractException.ExceptionType);
            await Verify(HttpStatusCode.ProxyAuthenticationRequired, FulcrumUnauthorizedException.ExceptionType);
            await Verify(HttpStatusCode.RequestTimeout, FulcrumTryAgainException.ExceptionType);
            await Verify(HttpStatusCode.Conflict, FulcrumConflictException.ExceptionType);
            await Verify(HttpStatusCode.Gone, FulcrumNotFoundException.ExceptionType);
            // This will represent 4xx
            await Verify(HttpStatusCode.ExpectationFailed, FulcrumServiceContractException.ExceptionType);
        }

        [TestMethod]
        public async Task StatusCode5xx()
        {
            await Verify(HttpStatusCode.InternalServerError, FulcrumAssertionFailedException.ExceptionType);
            await Verify(HttpStatusCode.NotImplemented, FulcrumNotImplementedException.ExceptionType);
            await Verify(HttpStatusCode.BadGateway, FulcrumResourceErrorException.ExceptionType);
            await Verify(HttpStatusCode.ServiceUnavailable, FulcrumTryAgainException.ExceptionType);
            await Verify(HttpStatusCode.GatewayTimeout, FulcrumTryAgainException.ExceptionType);
            // This will represent 5xx
            // Compilation error for LoopDetected?
            // await Verify(HttpStatusCode.LoopDetected, FulcrumAssertionFailedException.ExceptionType);
        }

        [TestMethod]
        public async Task ContentIsUsed()
        {
            var responseBodyContent = Guid.NewGuid().ToString();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(responseBodyContent, Encoding.UTF8)
            };
            var fulcrumError = await ExceptionConverter.ToFulcrumErrorAsync(responseMessage);
            Assert.IsNotNull(fulcrumError);
            Assert.IsTrue(fulcrumError.TechnicalMessage.Contains(responseBodyContent));
        }

        [TestMethod]
        public async Task ContentIsTruncated()
        {
            var responseBodyContent = new string('x', 1000);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(responseBodyContent, Encoding.UTF8)
            };
            var fulcrumError = await ExceptionConverter.ToFulcrumErrorAsync(responseMessage);
            Assert.IsNotNull(fulcrumError);
            Assert.IsTrue(fulcrumError.TechnicalMessage.Length < 1000);
            var smallContent = responseBodyContent.Substring(0,100);
            Assert.IsTrue(fulcrumError.TechnicalMessage.Contains(smallContent));
        }

        private static async Task Verify(HttpStatusCode statusCode, string expectedType)
        {
            var responseMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("Response body content", Encoding.UTF8)
            };
            var fulcrumError = await ExceptionConverter.ToFulcrumErrorAsync(responseMessage);
            Assert.IsNotNull(fulcrumError);
            Assert.AreEqual(expectedType, fulcrumError.Type);
        }
    }
}
