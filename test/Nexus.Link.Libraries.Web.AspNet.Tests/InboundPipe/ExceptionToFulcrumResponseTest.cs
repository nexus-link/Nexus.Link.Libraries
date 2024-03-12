using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Shouldly;
#pragma warning disable CS0618

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.IO;
#else
using Microsoft.Owin.Testing;
using System.Web.Http.ExceptionHandling;
using Newtonsoft.Json;
using ExceptionHandlerContext = System.Web.Http.ExceptionHandling.ExceptionHandlerContext;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;
using System.Net;
using System.Text;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe
{
    [TestClass]
    public class ExceptionToFulcrumResponseTest
    {
        private HttpClient _httpClient;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(NexusTestContextHeaderTest).FullName);

            TestStartup.TranslatorService = new Mock<ITranslatorService>().Object;
            TestStartup.GetTranslatorClientName = () => Foo.ConsumerName;
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task ExceptionToFulcrumResponse_Given_ClientCanceled_Gives_ServerMethodThrowsTaskCanceledException()
        {

#if NETCOREAPP
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();
#else
            _httpClient = TestServer.Create<TestStartup>().HttpClient;
#endif
            var countBefore = FoosController.ExecutionCount;
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Delay?delayMilliseconds=1000");

            var cts = new CancellationTokenSource();

            var task = _httpClient.SendAsync(request, cts.Token);

            while (!FoosController.DelayMethodStarted) await Task.Delay(1);
            cts.Cancel(true);
            await task.ShouldThrowAsync<TaskCanceledException>();

            var i = 0;
            while (FoosController.ExecutionCount == countBefore && ++i < 100) await Task.Delay(1);
            FoosController.LatestException.ShouldNotBeNull();
            FoosController.LatestException.ShouldBeAssignableTo<OperationCanceledException>();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task ExceptionToFulcrumResponse_Given_InternalCancellation_Gives_ServerMethodThrowsTaskCanceledException()
        {

#if NETCOREAPP
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();
#else
            _httpClient = TestServer.Create<TestStartup>().HttpClient;
#endif
            var countBefore = FoosController.ExecutionCount;
            FoosController.LatestInternalCancellationTokenSource = null;
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Delay?delayMilliseconds=1000");

            var cts = new CancellationTokenSource();
            var task = _httpClient.SendAsync(request, cts.Token);
            // Wait for the controller to create a token source
            while (FoosController.LatestInternalCancellationTokenSource == null) await Task.Delay(1);
            // Trigger a cancel on the token source
            FoosController.LatestInternalCancellationTokenSource.Cancel(true);
            // Wait for the controller to end
            var i = 0;
            while (FoosController.ExecutionCount == countBefore && ++i < 1100) await Task.Delay(1);
            await task.ShouldThrowAsync<OperationCanceledException>();
            FoosController.LatestRequestCancellationToken.ShouldNotBeNull();
            FoosController.LatestRequestCancellationTokenIsCancellationRequested.ShouldBe(false);
            FoosController.LatestException.ShouldNotBeNull();
            FoosController.LatestException.ShouldBeAssignableTo<OperationCanceledException>();
            await task.ShouldThrowAsync<TaskCanceledException>();
        }


        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task ExceptionToFulcrumResponse_Given_ClientCanceledAndNoStopwatch_Gives_500()
        {
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var handler = new ExceptionToFulcrumResponse(httpContext => throw new OperationCanceledException("operation cancelled"));
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var handler = new ExceptionToFulcrumResponse
            {
                InnerHandler = new ThrowOperationCancelledException()
                {
                    InnerHandler = new Mock<HttpMessageHandler>().Object
                }
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif


#if NETCOREAPP
            await handler.InvokeAsync(context);
            context.Response.StatusCode.ShouldBe(500);
#else
            var response = await invoker.SendAsync(request, CancellationToken.None);
            response.ShouldNotBeNull();
            ((int)response.StatusCode).ShouldBe(500);
#endif
        }


        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task ExceptionToFulcrumResponse_Given_ClientCanceledAndStopwatch_Gives_499()
        {
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var handler = new ExceptionToFulcrumResponse(httpContext => throw new OperationCanceledException("operation cancelled"));
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var handler = new ExceptionToFulcrumResponse
            {
                InnerHandler = new ThrowOperationCancelledException()
                {
                    InnerHandler = new Mock<HttpMessageHandler>().Object
                }
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif

            FulcrumApplication.Context.RequestStopwatch = new Stopwatch();
            FulcrumApplication.Context.RequestStopwatch.Start();


#if NETCOREAPP
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            context.RequestAborted = cancellationTokenSource.Token;
            await handler.InvokeAsync(context);
            context.Response.StatusCode.ShouldBe(499);
#else
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var response = await invoker.SendAsync(request, cancellationTokenSource.Token);
            response.ShouldNotBeNull();
            ((int)response.StatusCode).ShouldBe(499);
#endif
        }

#if NETCOREAPP
#else
        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task ConvertExceptionToFulcrumResponse_Given_ClientCanceledAndNoStopwatch_Gives_500()
        {
            // Arrange
            var exceptionHandler = new ConvertExceptionToFulcrumResponse();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var exceptionHandlerContext = new ExceptionHandlerContext(new ExceptionContext(
                new OperationCanceledException("Operaton cancelled"),
                new ExceptionContextCatchBlock("block", true, false)));

            // Act
            await exceptionHandler.HandleAsync(exceptionHandlerContext, cancellationTokenSource.Token);

            // Assert
            var response = await exceptionHandlerContext.Result.ExecuteAsync(CancellationToken.None);
            ((int)response.StatusCode).ShouldBe(500);
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task ConvertExceptionToFulcrumResponse_Given_ClientCanceledAndStopwatch_Gives_499()
        {
            //Arrange
            var exceptionHandler = new ConvertExceptionToFulcrumResponse();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var exceptionHandlerContext = new ExceptionHandlerContext(new ExceptionContext(
                new OperationCanceledException("Operaton cancelled"),
                new ExceptionContextCatchBlock("block", true, false)));

            FulcrumApplication.Context.RequestStopwatch = new Stopwatch();
            FulcrumApplication.Context.RequestStopwatch.Start();

            await exceptionHandler.HandleAsync(exceptionHandlerContext, cancellationTokenSource.Token);

            var response = await exceptionHandlerContext.Result.ExecuteAsync(CancellationToken.None);
            ((int)response.StatusCode).ShouldBe(499);
        }
#endif
#if NETCOREAPP
        private void SetRequest(HttpContext context, string url)
        {
            var request = context.Request;
            var match = Regex.Match(url, "^(https?)://([^/]+)(/[^?]+)(\\?.*)?$");
            request.Scheme = match.Groups[1].Value;
            request.Host = new HostString(match.Groups[2].Value);
            request.PathBase = new PathString("/");
            request.Path = new PathString(match.Groups[3].Value);
            request.Method = "GET";
            request.Body = new MemoryStream();
            request.QueryString = new QueryString(match.Groups[4].Value);
        }
#else
        private class ThrowOperationCancelledException : DelegatingHandler
        {
            public ThrowOperationCancelledException()
            {
            }

            /// <inheritdoc />
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                throw new OperationCanceledException("operation cancelled");
            }
        }
#endif
    }
}