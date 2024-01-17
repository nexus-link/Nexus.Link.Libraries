using System;
using System.Collections.Generic;
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
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

//TODO: ExceptionToFulcrumResponse

namespace Nexus.Link.Libraries.Web.AspNet.Tests.AspNetExceptionConverterTests
{
    [TestClass]
    public class FulcrumExceptionToResponseTests
    {

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void RunBeforeEveryTestCase()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(FulcrumExceptionToResponseTests).FullName);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public async Task Null()
        {
#if NETCOREAPP
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(null, null);
#else
            AspNetExceptionConverter.ToHttpResponseMessage(null);
            await Task.CompletedTask;
#endif
        }

        [TestMethod]
        public async Task RequestAcceptedException()
        {
            var url = Guid.NewGuid().ToString();
            var exception = new RequestAcceptedException(url);
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var result = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, result);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.Accepted, (int) result.StatusCode);
        }

        [TestMethod]
        public async Task RequestPostponedException()
        {
            var id = Guid.NewGuid().ToString();
            var exception = new TestRequestPostponedException(id);
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var result = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, result);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int)HttpStatusCode.Accepted, (int)result.StatusCode);
        }

        [TestMethod]
        public async Task RequestAggregatedPostponedException()
        {
            var id = Guid.NewGuid().ToString();
            var exception = new TestRequestPostponedException(id);
            exception.WaitingForRequestIds = new List<string> { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            exception.TryAgainAfterMinimumTimeSpan = TimeSpan.FromSeconds(10);
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var result = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, result);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int)HttpStatusCode.Accepted, (int)result.StatusCode);
        }

        [TestMethod]
        public async Task RequestRedirectedException()
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
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var actualResponse = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, actualResponse);
            Assert.AreEqual((int)expectedStatusCode, actualResponse.StatusCode);
            Assert.IsTrue(actualResponse.Headers.ContainsKey("Location"));
            Assert.AreEqual(expectedLocationUrl, actualResponse.Headers["Location"].FirstOrDefault());
#else
            var actualResponse = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            var actualContent = await actualResponse.Content.ReadAsStringAsync();
            Assert.AreEqual((int)expectedStatusCode, (int)actualResponse.StatusCode);
            Assert.AreEqual(expectedLocationUrl, actualResponse.Headers.Location.OriginalString);
            Assert.AreEqual(expectedContent, actualContent);
#endif
        }

        [TestMethod]
        public async Task FulcrumResourceLockedException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new FulcrumResourceLockedException(message);
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var result = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, result);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) (HttpStatusCode)423, (int) result.StatusCode);
        }

        [TestMethod]
        public async Task FulcrumAssertionException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new FulcrumAssertionFailedException(message);
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var result = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, result);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, (int)result.StatusCode);
        }

        [TestMethod]
        public async Task ArgumentNullException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new ArgumentNullException("the-parameter-name", message);
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var result = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, result);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.InternalServerError, (int) result.StatusCode);
        }
    }

    /// <summary>
    /// To be used internally as a concrete implementation
    /// </summary>
    public class TestRequestPostponedException : RequestPostponedException
    {
        public TestRequestPostponedException(params string[] postponeInfoWaitingForRequestIds)
            : base(postponeInfoWaitingForRequestIds)
        {
        }
    }
}
