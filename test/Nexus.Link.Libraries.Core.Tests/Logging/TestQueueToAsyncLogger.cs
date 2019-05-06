using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Logging
{
    [TestClass]
    public class TestQueueToAsyncLogger
    {
        private LogRecord _loggedRecord;
        private string _loggedCorrelationId;
        private QueueToAsyncLogger _queueLogger;
        private int _callsToFallback;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestLogHelper).FullName);

            // Mock fallback logger
            _callsToFallback = 0;
            var fallbackLoggerMock = new Mock<IFallbackLogger>();
            fallbackLoggerMock
                .Setup(logger => logger.SafeLog(It.IsAny<LogSeverityLevel>(), It.IsAny<string>()))
                .Callback((LogSeverityLevel sl, string m) => Interlocked.Increment(ref _callsToFallback));
            FulcrumApplication.Setup.FallbackLogger = fallbackLoggerMock.Object;

            // Mock async logger
            var asyncLoggerMock = new Mock<IAsyncLogger>();
            asyncLoggerMock
                .Setup(logger => logger.LogAsync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => {
                    _loggedRecord = logRecord;
                    _loggedCorrelationId = FulcrumApplication.Context.CorrelationId;
                })
                .Returns(Task.CompletedTask);
            _queueLogger = new QueueToAsyncLogger(asyncLoggerMock.Object);
            _queueLogger.KeepQueueAliveTimeSpan = TimeSpan.Zero;
        }

        [TestMethod]
        public void LogRecordIsUnchanged()
        {
            var expectedLogRecord = new LogRecord()
            {
                Message = Guid.NewGuid().ToString(),
                SeverityLevel = LogSeverityLevel.Error,
                Exception = new TestException("test"),
                TimeStamp = DateTimeOffset.Now,
                Location = Guid.NewGuid().ToString(),
                StackTrace = Guid.NewGuid().ToString()
            };
            _queueLogger.LogSync(expectedLogRecord);
            while (_queueLogger.OnlyForUnitTest_HasBackgroundWorkerForLogging) Thread.Sleep(10);
            UT.Assert.AreEqual(expectedLogRecord, _loggedRecord);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void CorrelationIdIsPreserved()
        {
            var expectedLogRecord = new LogRecord()
            {
                Message = Guid.NewGuid().ToString(),
                SeverityLevel = LogSeverityLevel.Error,
                TimeStamp = DateTimeOffset.Now
            };
            var expectedCorrelationId1 = Guid.NewGuid().ToString();
            FulcrumApplication.Context.CorrelationId = expectedCorrelationId1;
            _queueLogger.LogSync(expectedLogRecord);
            while (_queueLogger.OnlyForUnitTest_HasBackgroundWorkerForLogging) Thread.Sleep(10);
            UT.Assert.AreEqual(expectedCorrelationId1, _loggedCorrelationId);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        private class TestException : Exception
        {
            public TestException(string message) : base(message)
            {
            }
        }
    }
}
