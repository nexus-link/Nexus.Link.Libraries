using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
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
        public async Task StatusCode200()
        {
            // Represents 3xx
            await VerifyToFulcrumError(HttpStatusCode.OK, null);
        }

        [TestMethod]
        public async Task StatusCode300()
        {
            // Represents 3xx

            // 300
            await VerifyToFulcrumException(HttpStatusCode.Ambiguous, FulcrumHttpRedirectException.ExceptionType);
            await VerifyToFulcrumException(HttpStatusCode.MultipleChoices, FulcrumHttpRedirectException.ExceptionType);

            // 301
            await VerifyToFulcrumException(HttpStatusCode.MovedPermanently, FulcrumHttpRedirectException.ExceptionType);

            // 302
            await VerifyToFulcrumException(HttpStatusCode.Redirect, FulcrumHttpRedirectException.ExceptionType);
            await VerifyToFulcrumException(HttpStatusCode.Found, FulcrumHttpRedirectException.ExceptionType);

            // 303
            await VerifyToFulcrumException(HttpStatusCode.SeeOther, FulcrumHttpRedirectException.ExceptionType);

            // 304
            await VerifyToFulcrumException(HttpStatusCode.NotModified, FulcrumHttpRedirectException.ExceptionType);

            // 305
            await VerifyToFulcrumException(HttpStatusCode.UseProxy, FulcrumHttpRedirectException.ExceptionType);

            // 307
            await VerifyToFulcrumException(HttpStatusCode.TemporaryRedirect, FulcrumHttpRedirectException.ExceptionType);
        }

        [TestMethod]
        public async Task StatusCode400()
        {
            await VerifyToFulcrumError(HttpStatusCode.BadRequest, FulcrumServiceContractException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.Unauthorized, FulcrumUnauthorizedException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.PaymentRequired, FulcrumServiceContractException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.Forbidden, FulcrumForbiddenAccessException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.NotFound, FulcrumServiceContractException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.MethodNotAllowed, FulcrumServiceContractException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.NotAcceptable, FulcrumServiceContractException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.ProxyAuthenticationRequired, FulcrumUnauthorizedException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.RequestTimeout, FulcrumTryAgainException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.Conflict, FulcrumConflictException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.Gone, FulcrumNotFoundException.ExceptionType);
            // This will represent 4xx
            await VerifyToFulcrumError(HttpStatusCode.ExpectationFailed, FulcrumServiceContractException.ExceptionType);
        }

        [TestMethod]
        public async Task StatusCode500()
        {
            await VerifyToFulcrumError(HttpStatusCode.InternalServerError, FulcrumAssertionFailedException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.NotImplemented, FulcrumNotImplementedException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.BadGateway, FulcrumResourceException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.ServiceUnavailable, FulcrumTryAgainException.ExceptionType);
            await VerifyToFulcrumError(HttpStatusCode.GatewayTimeout, FulcrumTryAgainException.ExceptionType);
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

        private static async Task VerifyToFulcrumError(HttpStatusCode statusCode, string expectedType)
        {
            var responseMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("Response body content", Encoding.UTF8)
            };
            var fulcrumException = await ExceptionConverter.ToFulcrumErrorAsync(responseMessage);
            if (expectedType == null)
            {
                Assert.IsNull(fulcrumException);
            }
            else
            {
                Assert.IsNotNull(fulcrumException);
                Assert.AreEqual(expectedType, fulcrumException.Type);
            }
        }

        private static async Task VerifyToFulcrumException(HttpStatusCode statusCode, string expectedType)
        {
            var responseMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent("Response body content", Encoding.UTF8)
            };
            var fulcrumException = await ExceptionConverter.ToFulcrumExceptionAsync(responseMessage);
            if (expectedType == null)
            {
                Assert.IsNull(fulcrumException);
            }
            else
            {
                Assert.IsNotNull(fulcrumException);
                Assert.AreEqual(expectedType, fulcrumException.Type);
            }
        }
    }
}
