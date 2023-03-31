using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Translation;
using Shouldly;

#pragma warning disable CS0618

#if NETCOREAPP
#else
using Microsoft.Owin.Testing;
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
            await Task.Delay(10);
            cts.Cancel();
            await task.ShouldThrowAsync<TaskCanceledException>();
            while (FoosController.ExecutionCount == countBefore) await Task.Delay(1);
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
            FoosController.LatestInternalCancellationTokenSource.Cancel();
            // Wait for the controller to end
            while (FoosController.ExecutionCount == countBefore) await Task.Delay(1);
            await task.ShouldThrowAsync<OperationCanceledException>();
            FoosController.LatestRequestCancellationToken.ShouldNotBeNull();
            FoosController.LatestRequestCancellationToken.Value.IsCancellationRequested.ShouldBe(false);
            FoosController.LatestException.ShouldNotBeNull();
            FoosController.LatestException.ShouldBeAssignableTo<OperationCanceledException>();
            await task.ShouldThrowAsync<TaskCanceledException>();
        }
    }
}