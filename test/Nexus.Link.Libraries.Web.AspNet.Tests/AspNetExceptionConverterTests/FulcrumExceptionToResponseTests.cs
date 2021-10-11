using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;


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
        public void Null()
        {
#if NETCOREAPP
            AspNetExceptionConverter.ToContentResult(null);
#else
            AspNetExceptionConverter.ToHttpResponseMessage(null);
#endif
        }

        [TestMethod]
        public void RequestAcceptedException()
        {
            var url = Guid.NewGuid().ToString();
            var exception = new RequestAcceptedException(url);
#if NETCOREAPP
            var result = AspNetExceptionConverter.ToContentResult(exception);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.Accepted, (int) result.StatusCode);
        }

        [TestMethod]
        public void RequestPostponedException()
        {
            var id = Guid.NewGuid().ToString();
            var exception = new RequestPostponedException(id);
#if NETCOREAPP
            var result = AspNetExceptionConverter.ToContentResult(exception);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.Accepted, (int) result.StatusCode);
        }

        [TestMethod]
        public void FulcrumAssertionException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new FulcrumAssertionFailedException(message);
#if NETCOREAPP
            var result = AspNetExceptionConverter.ToContentResult(exception);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.InternalServerError, (int) result.StatusCode);
        }

        [TestMethod]
        public void ArgumentNullException()
        {
            var message = Guid.NewGuid().ToString();
            var exception = new ArgumentNullException("the-parameter-name", message);
#if NETCOREAPP
            var result = AspNetExceptionConverter.ToContentResult(exception);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int) HttpStatusCode.InternalServerError, (int) result.StatusCode);
        }
    }
}
