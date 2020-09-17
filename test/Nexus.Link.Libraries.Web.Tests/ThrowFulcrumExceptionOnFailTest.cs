using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
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

        [TestMethod]
        public async Task Success()
        {
            var handler =
                new ThrowFulcrumExceptionOnFail { UnitTest_SendAsyncDependencyInjection = SendAsyncSuccess };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/exception");
            await handler.SendAsync(request);
        }

        private static Task<HttpResponseMessage> SendAsyncTaskCanceledException(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new TaskCanceledException();
        }

        private static Task<HttpResponseMessage> SendAsyncSuccess(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Random content message", Encoding.UTF8)
            };
            return Task.FromResult(response);
        }
    }
}
