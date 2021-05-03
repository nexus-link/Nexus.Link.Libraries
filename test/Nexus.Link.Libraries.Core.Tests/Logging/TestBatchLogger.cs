using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Threads;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Logging
{
    [TestClass]
    public class TestBatchLogger
    {
        private int _loggedRecords;
        private Mock<ISyncLogger> _mockLogger;
        private int _callsToFallback;

        [TestInitialize]
        public void Initialize()
        {
            _callsToFallback = 0;
            var fallbackLoggerMock = new Mock<IFallbackLogger>();
            fallbackLoggerMock
                .Setup(logger => logger.SafeLog(It.IsAny<LogSeverityLevel>(), It.IsAny<string>()))
                .Callback((LogSeverityLevel sl, string m) => Interlocked.Increment(ref _callsToFallback));
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestBatchLogger).FullName);
            _loggedRecords = 0;
            _mockLogger = new Mock<ISyncLogger>();
            _mockLogger.Setup(logger =>
                logger.LogSync(It.IsAny<LogRecord>())).Callback((LogRecord _) => Interlocked.Increment(ref _loggedRecords));
            FulcrumApplication.Setup.SynchronousFastLogger = new BatchLogger(_mockLogger.Object);
            FulcrumApplication.Setup.FallbackLogger = fallbackLoggerMock.Object;
        }

        /// <summary>
        /// No log has a severity level over the individual threshold.
        /// </summary>
        [TestMethod]
        public void LogNone()
        {
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Critical;
            BatchLogger.StartBatch(LogSeverityLevel.None, true);
            Log.LogVerbose("Verbose");
            Log.LogInformation("Information");
            Log.LogWarning("Warning");
            Log.LogError("Error");
            UT.Assert.AreEqual(0, _loggedRecords);
            BatchLogger.EndBatch();

            UT.Assert.AreEqual(0, _loggedRecords);
            UT.Assert.AreEqual(0, _callsToFallback);
            _mockLogger.Verify();
        }

        /// <summary>
        /// Some logs has a severity level over the individual threshold.
        /// </summary>
        [TestMethod]
        public void LogOnlyOverThreshold()
        {
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Warning;
            BatchLogger.StartBatch(LogSeverityLevel.None, true);
            Log.LogVerbose("Verbose");
            Log.LogInformation("Information");
            Log.LogWarning("Warning");
            Log.LogError("Error");
            Log.LogCritical("Critical");
            UT.Assert.AreEqual(0, _loggedRecords);
            BatchLogger.EndBatch();
            UT.Assert.AreEqual(3, _loggedRecords);
            UT.Assert.AreEqual(0, _callsToFallback);
            _mockLogger.Verify();
        }

        /// <summary>
        /// Some logs has a severity level over the individual threshold.
        /// </summary>
        [TestMethod]
        public void CanCallEndBatchWithoutStartBatch()
        {
            BatchLogger.EndBatch();
            UT.Assert.AreEqual(0, _loggedRecords);
            UT.Assert.AreEqual(0, _callsToFallback);
            _mockLogger.Verify();
        }

        /// <summary>
        /// Some logs has a severity level over the individual threshold.
        /// </summary>
        [TestMethod]
        public void LogEverythingIfNotStarted()
        {
            UT.Assert.AreEqual(0, _loggedRecords);
            Log.LogVerbose("Verbose");
            UT.Assert.AreEqual(1, _loggedRecords);
            Log.LogInformation("Information");
            UT.Assert.AreEqual(2, _loggedRecords);
            Log.LogWarning("Warning");
            UT.Assert.AreEqual(3, _loggedRecords);
            Log.LogError("Error");
            UT.Assert.AreEqual(4, _loggedRecords);
            Log.LogCritical("Critical");
            UT.Assert.AreEqual(5, _loggedRecords);
            UT.Assert.AreEqual(0, _callsToFallback);
            _mockLogger.Verify();
        }

        /// <summary>
        /// "All threshold" is passed - all logs will be used, even those below individual threshold. They will not be flushed until the EndBatch().
        /// </summary>
        [TestMethod]
        public void LogAllOverThreshold()
        {
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Warning;
            BatchLogger.StartBatch(LogSeverityLevel.Error, true);
            Log.LogVerbose("Verbose");
            Log.LogInformation("Information");
            Log.LogWarning("Warning");
            Log.LogError("Error");
            Log.LogCritical("Critical");
            UT.Assert.AreEqual(0, _loggedRecords);
            BatchLogger.EndBatch();
            UT.Assert.AreEqual(5, _loggedRecords);
            UT.Assert.AreEqual(0, _callsToFallback);
            _mockLogger.Verify();
        }

        /// <summary>
        /// "All threshold" is passed - all logs will be used, even those below individual threshold. They will not be flushed as soon as the threshold is met.
        /// </summary>
        [TestMethod]
        public void LogAllImmediatelyWhenOverThreshold()
        {
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Warning;
            BatchLogger.StartBatch(LogSeverityLevel.Error, false);
            Log.LogVerbose("Verbose");
            Log.LogInformation("Information");
            Log.LogWarning("Warning");
            UT.Assert.AreEqual(0, _loggedRecords);
            Log.LogError("Error");
            UT.Assert.AreEqual(4, _loggedRecords);
            Log.LogVerbose("Verbose");
            UT.Assert.AreEqual(5, _loggedRecords);
            BatchLogger.EndBatch();
            UT.Assert.AreEqual(5, _loggedRecords);
            UT.Assert.AreEqual(0, _callsToFallback);
            _mockLogger.Verify();
        }

        /// <summary>
        /// First log all records, then just log some
        /// </summary>
        [TestMethod]
        public void LogAllThenLogSome()
        {
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Warning;
            BatchLogger.StartBatch(LogSeverityLevel.Error, false);
            Log.LogVerbose("Verbose");
            Log.LogInformation("Information");
            Log.LogWarning("Warning");
            UT.Assert.AreEqual(0, _loggedRecords);
            Log.LogError("Error");
            UT.Assert.AreEqual(4, _loggedRecords);
            Log.LogVerbose("Verbose");
            UT.Assert.AreEqual(5, _loggedRecords);
            BatchLogger.EndBatch();

            _loggedRecords = 0;
            BatchLogger.StartBatch(LogSeverityLevel.Error, false);
            Log.LogVerbose("Verbose");
            Log.LogInformation("Information");
            Log.LogWarning("Warning");
            UT.Assert.AreEqual(0, _loggedRecords);
            BatchLogger.EndBatch();
            UT.Assert.AreEqual(1, _loggedRecords);
            UT.Assert.AreEqual(0, _callsToFallback);

            _mockLogger.Verify();
        }

        //
        // NOTE! Has to be run separately: TODO: fix this
        //
        /// <summary>
        /// Make sure BatchLogger handles underlying collection properly
        /// </summary>
        [TestMethod]
        public void HandlesUnderlyingCollectionProperly()
        {
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Warning;
            BatchLogger.StartBatch(LogSeverityLevel.Warning, true);

            for (var i = 1; i <= 10; i++) Log.LogWarning($"Warning A {i}");

            var allDone = new ManualResetEvent(false);

            // Try to provoke "System.InvalidOperationException: Collection was modified; enumeration operation may not execute."
            // Prior to version 4.15.3 this would trigger the exception:
            ThreadHelper.FireAndForget(() =>
            {
                for (var i = 1; i <= 100000; i++) Log.LogWarning($"Warning B {i}");
                allDone.Set();
            });
            BatchLogger.EndBatch();

            UT.Assert.IsTrue(allDone.WaitOne(TimeSpan.FromSeconds(10)));
            UT.Assert.AreEqual(0, _callsToFallback, $"{nameof(_loggedRecords)}: {_loggedRecords}");
            UT.Assert.IsTrue(_loggedRecords >= 100, _loggedRecords.ToString());
        }
    }
}
