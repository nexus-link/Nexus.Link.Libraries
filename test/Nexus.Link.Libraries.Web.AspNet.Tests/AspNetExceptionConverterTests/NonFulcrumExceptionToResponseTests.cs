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
using System.Threading;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Tests.AspNetExceptionConverterTests
{
    [TestClass]
    public class NonFulcrumExceptionToResponseTests
    {

        [TestInitialize]
        public void RunBeforeEveryTestCase()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(FulcrumExceptionToResponseTests).FullName);
        }

        [TestMethod]
        public async Task Given_ClientCancelled_Gives_HttpStatus400()
        {
            var exception = new OperationCanceledException();
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var result = context.Response;
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, result);
#else
            var result = AspNetExceptionConverter.ToHttpResponseMessage(exception);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int)HttpStatusCode.BadRequest, (int)result.StatusCode);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task Given_Timeout_Gives_HttpStatus500()
        {
            var cts = new CancellationTokenSource(1);
            Exception exception = null;
            try
            {
                await Task.Delay(100, cts.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);

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
            // TODO: More assertions
        }
    }
}