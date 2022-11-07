#if NETCOREAPP
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;
using System.Linq;

using Microsoft.AspNetCore.Http;

//TODO: ExceptionToFulcrumResponse

namespace Nexus.Link.Libraries.Web.AspNet.Tests.AspNetExceptionConverterTests
{
    [TestClass]
    public class FulcrumExceptionToCustomHttpResponseTests
    {

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void RunBeforeEveryTestCase()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(FulcrumExceptionToResponseTests).FullName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public void Null()
        {
            AspNetExceptionConverter.ConvertExceptionToCustomHttpResponse(null);
        }

        [TestMethod]
        public void RequestAcceptedException()
        {
            var url = Guid.NewGuid().ToString();
            var exception = new RequestAcceptedException(url);
            var result = AspNetExceptionConverter.ConvertExceptionToCustomHttpResponse(exception);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.Accepted, (int) result.StatusCode);
        }

        [TestMethod]
        public void RequestPostponedException()
        {
            var id = Guid.NewGuid().ToString();
            var exception = new RequestPostponedException(id);
            var result = AspNetExceptionConverter.ConvertExceptionToCustomHttpResponse(exception);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int)HttpStatusCode.Accepted, (int)result.StatusCode);
        }

        [TestMethod]
        public void RequestRedirectedException()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://old.example.com/redirect-source");
            var expectedStatusCode = HttpStatusCode.Redirect;
            var responseMessage = new HttpResponseMessage(expectedStatusCode)
            {
                RequestMessage = requestMessage
            };
            var expectedLocationUrl = "https://new.example.com/redirect-target";
            var expectedContent = Guid.NewGuid().ToString();
            responseMessage.Headers.Location = new Uri(expectedLocationUrl);
            var exception = new FulcrumHttpRedirectException(responseMessage, expectedContent);
            var actualResponse = AspNetExceptionConverter.ConvertExceptionToCustomHttpResponse(exception);
            Assert.AreEqual((int)expectedStatusCode, actualResponse.StatusCode);
            Assert.AreEqual(expectedLocationUrl, actualResponse.LocationUri.OriginalString);
        }

        [TestMethod]
        public void FulcrumResourceLockedException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new FulcrumResourceLockedException(message);
            var result = AspNetExceptionConverter.ConvertExceptionToCustomHttpResponse(exception);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) (HttpStatusCode)423, (int) result.StatusCode);
        }

        [TestMethod]
        public void FulcrumAssertionException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new FulcrumAssertionFailedException(message);
            var result = AspNetExceptionConverter.ConvertExceptionToCustomHttpResponse(exception);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, (int)result.StatusCode);
        }

        [TestMethod]
        public void ArgumentNullException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new ArgumentNullException("the-parameter-name", message);
            var result = AspNetExceptionConverter.ConvertExceptionToCustomHttpResponse(exception);
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.InternalServerError, (int) result.StatusCode);
        }
    }
}

#endif