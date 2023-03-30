using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe.Support;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.Pipe;
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
        public async Task ExceptionToFulcrumResponse_Given_ClientCancelled_Gives_ThrowsTaskCanceledException()
        {

#if NETCOREAPP
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();
#else
            _httpClient = TestServer.Create<TestStartup>().HttpClient;
#endif
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Delay?delayMilliseconds=60000");

            var cts = new CancellationTokenSource(100);

            var task = _httpClient.SendAsync(request, cts.Token);
            await Task.Delay(10);
            cts.Cancel();
            await task.ShouldThrowAsync<TaskCanceledException>();
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task ExceptionToFulcrumResponse_Given_ServerTimeout_Gives_HttpStatus500()
        {
            FulcrumApplication.Context.ValueProvider.SetValue<TimeSpan?>("KeepAliveTimeout", TimeSpan.FromMilliseconds(100));

#if NETCOREAPP
            var factory = new CustomWebApplicationFactory();
            _httpClient = factory.CreateClient();
#else
            _httpClient = TestServer.Create<TestStartup>().HttpClient;
#endif
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/Delay?delayMilliseconds=2000");

            var response = await _httpClient.SendAsync(request);

            var resultString = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode, resultString);
        }
    }
}