using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Text.RegularExpressions;
#else
using System.Net;
using System.Net.Http;
using System.Threading;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe
{
    [TestClass]
    public class LogRequestResponseTests
    {

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LogRequestResponseTests).FullName);
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task StatusCode200LogInformation()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 200);
            var outerHandler =
                new LogRequestAndResponse(innerHandler.InvokeAsync);
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var outerHandler = new LogRequestAndResponse()
            {
                InnerHandler = new ReturnResponseWithPresetStatusCode(HttpStatusCode.OK)
                {
                    InnerHandler = new Mock<HttpMessageHandler>().Object
                }
            };
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif


#if NETCOREAPP
            await outerHandler.InvokeAsync(context);
#else
            await invoker.SendAsync(request, CancellationToken.None);
#endif

            Assert.AreEqual(LogSeverityLevel.Information, highestSeverityLevel);
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task StatusCode400LogWarning()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 400);
            var outerHandler =
                new LogRequestAndResponse(innerHandler.InvokeAsync);
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var outerHandler = new LogRequestAndResponse()
            {
                InnerHandler = new ReturnResponseWithPresetStatusCode(HttpStatusCode.BadRequest)
                {
                    InnerHandler = new Mock<HttpMessageHandler>().Object
                }
            };
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif


#if NETCOREAPP
            await outerHandler.InvokeAsync(context);
#else
            await invoker.SendAsync(request, CancellationToken.None);
#endif

            Assert.AreEqual(LogSeverityLevel.Warning, highestSeverityLevel);
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task StatusCode500LogError()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 500);
            var outerHandler =
                new LogRequestAndResponse(innerHandler.InvokeAsync);
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var outerHandler = new LogRequestAndResponse()
            {
                InnerHandler = new ReturnResponseWithPresetStatusCode(HttpStatusCode.InternalServerError)
                {
                    InnerHandler = new Mock<HttpMessageHandler>().Object
                }
            };
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif


#if NETCOREAPP
            await outerHandler.InvokeAsync(context);
#else
            await invoker.SendAsync(request, CancellationToken.None);
#endif

            Assert.AreEqual(LogSeverityLevel.Error, highestSeverityLevel);
        }

#if NETCOREAPP
        private void SetRequest(DefaultHttpContext context, string url)
        {
            var request = new DefaultHttpRequest(context);
            var match = Regex.Match(url, "^(https?)://([^/]+)(/[^?]+)(\\?.*)?$");
            request.Scheme = match.Groups[1].Value;
            request.Host = new HostString(match.Groups[2].Value);
            request.PathBase = new PathString("/");
            request.Path = new PathString(match.Groups[3].Value);
            request.Method = "GET";
            request.Body = new MemoryStream();
            request.QueryString = new QueryString(match.Groups[4].Value);
        }

        private class ReturnResponseWithPresetStatusCode
        {
            private readonly int _statusCode;
            private readonly RequestDelegate _next;

            /// <inheritdoc />
            public ReturnResponseWithPresetStatusCode(RequestDelegate next, int statusCode)
            {
                _next = next;
                _statusCode = statusCode;
            }

            public Task InvokeAsync(HttpContext context)
            {
                context.Response.StatusCode = _statusCode;
                return Task.CompletedTask;
            }
        }
#else
        private class ReturnResponseWithPresetStatusCode : DelegatingHandler
        {
            private HttpStatusCode _statusCode;

            public ReturnResponseWithPresetStatusCode(HttpStatusCode statusCode)
            {
                _statusCode = statusCode;
            }

            /// <inheritdoc />
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(_statusCode));
            }
        }
#endif
    }
}
