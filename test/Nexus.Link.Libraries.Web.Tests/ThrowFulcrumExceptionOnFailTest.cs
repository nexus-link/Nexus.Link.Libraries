using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Tests.Support;

namespace Nexus.Link.Libraries.Web.Tests
{
    [TestClass]
    public class ThrowFulcrumExceptionOnFailTest
    {
        [TestInitialize]
        public void RunForEveryTestCase()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LogRequestAndResponseTest).FullName);
        }

        [TestMethod]
        public async Task TaskCanceledException()
        {
            var handler =
                new ThrowFulcrumExceptionOnFail { UnitTest_SendAsyncDependencyInjection = SendAsyncTaskCanceledException };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/exception");
            try
            {
                await handler.SendAsync(request);
                Assert.Fail("Expected an exception");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e as FulcrumTryAgainException, $"Expected exception of type {nameof(FulcrumTryAgainException)}, but was {e.GetType().FullName}");
            }
        }

        private static Task<HttpResponseMessage> SendAsyncTaskCanceledException(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new TaskCanceledException();
        }
    }
}
