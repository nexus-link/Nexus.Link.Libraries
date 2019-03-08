using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Tests.Support;

namespace Nexus.Link.Libraries.Web.Tests
{
    [TestClass]
    public class LogRequestAndResponseTest : ISyncLogger
    {
        private readonly Dictionary<LogSeverityLevel, string> _lastMessageDictionary =
            new Dictionary<LogSeverityLevel, string>();

        private int _numberOfLogs;
        private int _expectedNumberOfLogs;

        [TestInitialize]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LogRequestAndResponseTest).FullName);
            FulcrumApplication.Setup.SynchronousFastLogger = this;
        }

        [TestMethod]
        public async Task ResponseOk()
        {
            var logRequestAndResponse = new LogRequestAndResponse
            {
                UnitTest_SendAsyncDependencyInjection = SendAsyncResponseOk
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/okresponse");
            SetExpectedNumberOfLogs(1);
            await logRequestAndResponse.SendAsync(request);
            var lastMessage = LastMessage(LogSeverityLevel.Information);
            Assert.IsNotNull(lastMessage);
            Assert.IsTrue(lastMessage.Contains($"TEST request-response POST {request.RequestUri}"));
            Assert.IsTrue(lastMessage.Contains(request.RequestUri.ToString()));
        }

        [TestMethod]
        public async Task ResponseBadRequest()
        {
            var logRequestAndResponse = new LogRequestAndResponse
            {
                UnitTest_SendAsyncDependencyInjection = SendAsyncResponseBadRequest
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/badrequest");
            SetExpectedNumberOfLogs(1);
            await logRequestAndResponse.SendAsync(request);
            var lastMessage = LastMessage(LogSeverityLevel.Warning);
            Assert.IsNotNull(lastMessage);
            Assert.IsTrue(lastMessage.Contains($"TEST request-response POST {request.RequestUri}"));
        }

        [TestMethod]
        public async Task ResponseException()
        {
            var logRequestAndResponse =
                new LogRequestAndResponse {UnitTest_SendAsyncDependencyInjection = SendAsyncResponseException};
            var request = new HttpRequestMessage(HttpMethod.Post, "http://example.com/exception");
            SetExpectedNumberOfLogs(1);
            try
            {
                await logRequestAndResponse.SendAsync(request);
                Assert.Fail("Expected an exception");
            }
            catch (Exception)
            {
                var lastMessage = LastMessage(LogSeverityLevel.Error);
                Assert.IsNotNull(lastMessage);
                Assert.IsTrue(lastMessage.Contains($"TEST request-exception POST {request.RequestUri}"));
            }
        }

        private void SetExpectedNumberOfLogs(int expectedNumberOfLogs)
        {
            _expectedNumberOfLogs = expectedNumberOfLogs;
            _numberOfLogs = 0;
        }

        private string LastMessage(LogSeverityLevel severityLevel)
        {
            var count = 0;
            while (count++ < 10 && _numberOfLogs < _expectedNumberOfLogs) Thread.Sleep(TimeSpan.FromMilliseconds(100));
            Assert.IsFalse(_numberOfLogs < _expectedNumberOfLogs,
                $"Expected {_expectedNumberOfLogs} logs, got {_numberOfLogs}");
            while (count++ < 10 && _numberOfLogs <= _expectedNumberOfLogs &&
                   !_lastMessageDictionary.ContainsKey(severityLevel)) Thread.Sleep(TimeSpan.FromMilliseconds(100));
            Assert.IsFalse(_numberOfLogs > _expectedNumberOfLogs,
                $"Expected {_expectedNumberOfLogs} logs, got {_numberOfLogs}");
            return !_lastMessageDictionary.ContainsKey(severityLevel) ? null : _lastMessageDictionary[severityLevel];
        }

        private static async Task<HttpResponseMessage> SendAsyncResponseOk(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) {RequestMessage = request};
            return await Task.FromResult(response);
        }

        private static async Task<HttpResponseMessage> SendAsyncResponseBadRequest(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {RequestMessage = request};
            return await Task.FromResult(response);
        }

        private static Task<HttpResponseMessage> SendAsyncResponseException(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new ApplicationException("SendAsync failed");
        }

        public void Log(LogSeverityLevel logSeverityLevel, string message)
        {
            throw new NotImplementedException();
        }

        public void LogSync(LogRecord logRecord)
        {
            Console.WriteLine($"\r{logRecord.ToLogString(true)}\r");
            _lastMessageDictionary[logRecord.SeverityLevel] = logRecord.Message;
            _numberOfLogs++;
        }
    }
}
