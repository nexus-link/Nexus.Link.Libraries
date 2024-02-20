using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
using System.Threading;
using Shouldly;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using System.IO;
#else
using System.Net.Http;
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
        public async Task Given_InternalCancel_Gives_HttpStatus500()
        {
            var exception = new OperationCanceledException();
            var tokenSource = new CancellationTokenSource();
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var response = context.Response;
            response.Body = new MemoryStream();
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, response, tokenSource.Token);
#else
            var response = AspNetExceptionConverter.ToHttpResponseMessage(exception, tokenSource.Token);
            await Task.CompletedTask;
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, (int)response.StatusCode);
            var content = await GetContentAsync(response);
            content.ShouldNotContain(".FulcrumException");
        }

        [TestMethod]
        public async Task Given_ClientCancel_Gives_HttpStatus499()
        {
            FulcrumApplication.Context.RequestStopwatch = new Stopwatch();
            FulcrumApplication.Context.RequestStopwatch.Start();

            var exception = new OperationCanceledException();
            var tokenSource = new CancellationTokenSource(1);
            tokenSource.Cancel();

            string responseContent;
#if NETCOREAPP
            var context = new DefaultHttpContext();
            var response = context.Response;
            response.Body = new MemoryStream();
            await AspNetExceptionConverter.ConvertExceptionToResponseAsync(exception, response, tokenSource.Token);
            responseContent = await GetContentAsync(response);
#else
            var response = AspNetExceptionConverter.ToHttpResponseMessage(exception, tokenSource.Token);
            await Task.CompletedTask;
            responseContent = await response.Content.ReadAsStringAsync();
#endif
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual(499, (int)response.StatusCode, responseContent);
        }

#if NETCOREAPP
        private async Task<string> GetContentAsync(HttpResponse response)
        {
            // Read the stream as text
            response.Body.Position = 0;
            var content = await new StreamReader(response.Body).ReadToEndAsync();
            return content;
        }
#else
        private async Task<string> GetContentAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
#endif

    }
}