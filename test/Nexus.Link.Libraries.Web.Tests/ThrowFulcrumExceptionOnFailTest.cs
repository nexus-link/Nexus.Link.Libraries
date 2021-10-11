using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Error;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Tests.Pipe;
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
                new ThrowFulcrumExceptionOnFailForTest { UnitTest_SendAsyncDependencyInjection = SendAsyncTaskCanceledException };
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
        public async Task RequestAcceptedException()
        {
            var handler =
                new ThrowFulcrumExceptionOnFailForTest { UnitTest_SendAsyncDependencyInjection = SendAsyncRequestAcceptedException };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/exception");
            try
            {
                await handler.SendAsync(request);
                Assert.Fail("Expected an exception");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e as RequestAcceptedException, $"Expected exception of type {nameof(RequestAcceptedException)}, but was {e.GetType().FullName}");
            }
        }

        [TestMethod]
        public async Task RequestPostponedException()
        {
            var handler =
                new ThrowFulcrumExceptionOnFailForTest { UnitTest_SendAsyncDependencyInjection = SendAsyncRequestPostponedException };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/exception");
            try
            {
                await handler.SendAsync(request);
                Assert.Fail("Expected an exception");
            }
            catch (Exception e)
            {
                Assert.IsNotNull(e as RequestPostponedException, $"Expected exception of type {nameof(RequestPostponedException)}, but was {e.GetType().FullName}");
            }
        }

        [TestMethod]
        public async Task Success()
        {
            var handler =
                new ThrowFulcrumExceptionOnFailForTest { UnitTest_SendAsyncDependencyInjection = SendAsyncSuccess };
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

        private static Task<HttpResponseMessage> SendAsyncRequestAcceptedException(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var accept = new RequestAcceptedContent()
            {
                UrlWhereResponseWillBeMadeAvailable = Guid.NewGuid().ToString()
            };
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(JsonConvert.SerializeObject(accept), Encoding.UTF8)
            };
            return Task.FromResult(response);
        }

        private static Task<HttpResponseMessage> SendAsyncRequestPostponedException(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var accept = new RequestPostponedContent()
            {
                WaitingForRequestIds = new List<string> { Guid.NewGuid().ToString() }
            };
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(JsonConvert.SerializeObject(accept), Encoding.UTF8)
            };
            return Task.FromResult(response);
        }
    }
}
