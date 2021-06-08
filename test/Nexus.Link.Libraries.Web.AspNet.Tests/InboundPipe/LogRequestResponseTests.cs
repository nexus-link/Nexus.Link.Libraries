using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Nexus.Link.Libraries.Web.Error.Logic;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Text.RegularExpressions;
#pragma warning disable 618
#else
using System.Net;
using System.Net.Http;
using System.Text;
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
            mockLogger
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                });
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 200);
            var outerHandler = new LogRequestAndResponse(innerHandler.InvokeAsync);
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

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public async Task Require_Single_Handler_In_InPipe()
        {
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var innerHandler1 = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 200);
            var innerHandler2 = new LogRequestAndResponse(innerHandler1.InvokeAsync);
            var outerHandler = new LogRequestAndResponse(innerHandler2.InvokeAsync);
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var outerHandler = new LogRequestAndResponse()
            {
                InnerHandler = new LogRequestAndResponse()
            };
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif


#if NETCOREAPP
            await outerHandler.InvokeAsync(context);
#else
            await invoker.SendAsync(request, CancellationToken.None);
#endif
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

            public async Task InvokeAsync(HttpContext context)
            {
                context.Response.StatusCode = _statusCode;
                FulcrumException fulcrumException = null;
                if (_statusCode >= 500)
                {
                    fulcrumException = new FulcrumAssertionFailedException("Internal error message");
                }
                else if (_statusCode >= 400)
                {
                    fulcrumException = new FulcrumServiceContractException("Client error message");
                }

                if (_statusCode >= 400)
                {
                    await context.Response.WriteAsync("Test");
                    context.Response.Body = new MemoryStream();
                    context.Response.ContentType = "application/json";
                    var fulcrumError = ExceptionConverter.ToFulcrumError(fulcrumException);
                    var content = ExceptionConverter.ToJsonString(fulcrumError, Formatting.Indented);
                    await context.Response.WriteAsync(content);
                }
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
                var response = new HttpResponseMessage(_statusCode);
                if ((int)_statusCode >= 500)
                {
                    var fulcrumException = new FulcrumAssertionFailedException("Internal error message");
                    var fulcrumError = ExceptionConverter.ToFulcrumError(fulcrumException);
                    var content = ExceptionConverter.ToJsonString(fulcrumError, Formatting.Indented);
                    var stringContent = new StringContent(content, Encoding.UTF8);
                    response = new HttpResponseMessage(_statusCode)
                    {
                        Content = stringContent
                    };
                }
                else if ((int)_statusCode >= 400)
                {
                    var fulcrumException = new FulcrumServiceContractException("Client error message");
                    var fulcrumError = ExceptionConverter.ToFulcrumError(fulcrumException);
                    var content = ExceptionConverter.ToJsonString(fulcrumError, Formatting.Indented);
                    var stringContent = new StringContent(content, Encoding.UTF8);
                    response = new HttpResponseMessage(_statusCode)
                    {
                        Content = stringContent
                    };
                }

                return Task.FromResult(response);
            }
        }
#endif
    }
}
