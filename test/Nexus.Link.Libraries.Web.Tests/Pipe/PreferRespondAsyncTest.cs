using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Pipe;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.Tests.Support;

namespace Nexus.Link.Libraries.Web.Tests.Pipe
{
    [TestClass]
    public class PreferRespondAsyncTest
    {
        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(PreferRespondAsyncTest).FullName);
        }

        [TestMethod]
        public async Task Given_NotAsynchronous_Gives_NoHeader()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionIsAsynchronous = false;
            var handler = new PreferRespondAsyncForTest(true);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/api/Persons");

            // Act
            var response = await handler.SendAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(HasPreferRespondAsyncHeader(response.RequestMessage));
        }

        [TestMethod]
        public async Task Given_NotAsynchronousButOtherPreferHeader_Gives_NoHeader()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionIsAsynchronous = false;
            var handler = new PreferRespondAsyncForTest(true);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/api/Persons");
            request.Headers.Add(Constants.PreferHeaderName, "random");

            // Act
            var response = await handler.SendAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsFalse(HasPreferRespondAsyncHeader(response.RequestMessage));
        }

        [TestMethod]
        public async Task Given_Asynchronous_Gives_Header()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionIsAsynchronous = true;
            var handler = new PreferRespondAsyncForTest(true);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/api/Persons");

            // Act
            var response = await handler.SendAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(HasPreferRespondAsyncHeader(response.RequestMessage));
        }

        [TestMethod]
        public async Task Given_AsynchronousAndOtherPreferHeader_Gives_Header()
        {
            // Arrange
            FulcrumApplication.Context.ExecutionIsAsynchronous = true;
            var handler = new PreferRespondAsyncForTest(true);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/api/Persons");
            request.Headers.Add(Constants.PreferHeaderName, "random");

            // Act
            var response = await handler.SendAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(HasPreferRespondAsyncHeader(response.RequestMessage));
        }

        private static bool HasPreferRespondAsyncHeader(HttpRequestMessage request)
        {
            return request.Headers.TryGetValues(Constants.PreferHeaderName, out var preferHeader)
                   && preferHeader.Contains(Constants.PreferRespondAsyncHeaderValue);
        }
    }

    public class PreferRespondAsyncForTest : PreferRespondAsync
    {
        public PreferRespondAsyncForTest(bool fromText)
        {
            var innerHandlerMock = new Mock<PreferRespondAsyncForTest>();
            innerHandlerMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns((HttpRequestMessage r, CancellationToken ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    RequestMessage = r
                }));
            InnerHandler = innerHandlerMock.Object;
        }
        public PreferRespondAsyncForTest()
        {
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await base.SendAsync(request, CancellationToken.None);
        }
    }
}
