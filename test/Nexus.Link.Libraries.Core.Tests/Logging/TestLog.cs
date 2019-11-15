using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Logging
{
    [TestClass]
    public class TestLog
    {
        private int _callsToFallback;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestLog).FullName);
            _callsToFallback = 0;
            var fallbackLoggerMock = new Mock<IFallbackLogger>();
            fallbackLoggerMock
                .Setup(logger => logger.SafeLog(It.IsAny<LogSeverityLevel>(), It.IsAny<string>()))
                .Callback((LogSeverityLevel sl, string m) => Interlocked.Increment(ref _callsToFallback));
            FulcrumApplication.Setup.FallbackLogger = fallbackLoggerMock.Object;
        }

        [TestMethod]
        public void DetectRecursiveLogging()
        {
            FulcrumApplication.Setup.SynchronousFastLogger = new RecursiveSyncLogger();
            RecursiveSyncLogger.IsRunning = true;
            Log.LogInformation("Top level logging 1, will be followed by recursive logging that should be detected");
            while (RecursiveSyncLogger.IsRunning) Thread.Sleep(TimeSpan.FromMilliseconds(10));
            UT.Assert.IsFalse(RecursiveSyncLogger.HasFailed, RecursiveSyncLogger.Message);
            UT.Assert.AreEqual(1, _callsToFallback); // The log about the recursive call
        }

        [TestMethod]
        public void LogVerbose()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogVerbose("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Verbose, foundLogRecord.SeverityLevel);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogVerboseWithData()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogVerbose("test", new {a = 23});
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.IsNotNull(foundLogRecord.Data);
            UT.Assert.AreEqual(23, foundLogRecord.Data["a"]);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogInformation()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogInformation("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Information, foundLogRecord.SeverityLevel);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogOnlyOverThreshold()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Warning;
            Log.LogVerbose("test");
            UT.Assert.IsNull(foundLogRecord);
            Log.LogInformation("test");
            UT.Assert.IsNull(foundLogRecord);
            Log.LogWarning("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Warning, foundLogRecord.SeverityLevel);
            foundLogRecord = null;
            Log.LogError("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Error, foundLogRecord.SeverityLevel);
            foundLogRecord = null;
            Log.LogCritical("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Critical, foundLogRecord.SeverityLevel);
            foundLogRecord = null;
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogInformationWithData()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogInformation("test", new { a = 24 });
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.IsNotNull(foundLogRecord.Data);
            UT.Assert.AreEqual(24, foundLogRecord.Data["a"]);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogWarning()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogWarning("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Warning, foundLogRecord.SeverityLevel);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogWarningWithData()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogWarning("test", new { a = 25 });
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.IsNotNull(foundLogRecord.Data);
            UT.Assert.AreEqual(25, foundLogRecord.Data["a"]);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogError()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogError("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Error, foundLogRecord.SeverityLevel);
            UT.Assert.AreEqual(0, _callsToFallback);

        }

        [TestMethod]
        public void LogErrorWithData()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogError("test", new { a = 26 });
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(26, foundLogRecord.Data["a"]);
            UT.Assert.AreEqual(0, _callsToFallback);

        }

        [TestMethod]
        public void LogCritical()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogCritical("test");
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.AreEqual(LogSeverityLevel.Critical, foundLogRecord.SeverityLevel);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogCriticalWithData()
        {
            LogRecord foundLogRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => foundLogRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            Log.LogCritical("test", new { a = 27 });
            UT.Assert.IsNotNull(foundLogRecord);
            UT.Assert.IsNotNull(foundLogRecord.Data);
            UT.Assert.AreEqual(27, foundLogRecord.Data["a"]);
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LogOnLevel()
        {
            const string expectedMessage = "TestMessage";
            var expectedException = new TestException();
            LogRecord loggedRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => loggedRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            for (var expectedLevel = LogSeverityLevel.Verbose; expectedLevel <= LogSeverityLevel.Critical; expectedLevel++)
            {
                loggedRecord = null;
                Log.LogOnLevel(expectedLevel, expectedMessage, expectedException);
                UT.Assert.AreEqual(expectedLevel, loggedRecord?.SeverityLevel);
                UT.Assert.AreEqual(expectedMessage, loggedRecord?.Message);
                UT.Assert.AreEqual(expectedException, loggedRecord?.Exception);
                UT.Assert.IsTrue(loggedRecord.TimeStamp <= DateTimeOffset.Now);
                UT.Assert.IsTrue(loggedRecord.TimeStamp >= DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(5)));
            }
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        [TestMethod]
        public void LocationIsSet()
        {
            LogRecord loggedRecord = null;
            var syncLoggerMock = new Mock<ISyncLogger>();
            syncLoggerMock
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) => loggedRecord = logRecord);
            FulcrumApplication.Setup.SynchronousFastLogger = syncLoggerMock.Object;
            // ReSharper disable ExplicitCallerInfoArgument
            Log.LogOnLevel(LogSeverityLevel.Error, "Test", null, "memberName", "filePath", 42);
            UT.Assert.AreEqual(LogHelper.LocationToLogString("memberName", "filePath", 42), loggedRecord.Location);
            // ReSharper restore ExplicitCallerInfoArgument
            UT.Assert.AreEqual(0, _callsToFallback);
        }

        /// <summary>
        /// Verify that we don't need over loaded methods with IDictionary.
        /// </summary>
        [TestMethod]
        public void DictionaryIsTransformedToJObjectAsExpected()
        {
            var subData = new Dictionary<string, object>
            {
                { "a", 12 },
                { "b", 13 }
            };
            var data = new Dictionary<string, object>
            {
                { "number", 23 },
                { "boolean", false },
                { "object", subData }
            };
            var jObject = JObject.FromObject(data);
            var jsonString = jObject.ToString(Formatting.None);
            UT.Assert.AreEqual("{\"number\":23,\"boolean\":false,\"object\":{\"a\":12,\"b\":13}}", jsonString);
        }

        /// <summary>
        /// Verify that we don't need over loaded methods with JObject.
        /// </summary>
        [TestMethod]
        public void JObjectIsTransformedToJObjectAsExpected()
        {
            var subData = new Dictionary<string, object>
            {
                { "a", 12 },
                { "b", 13 }
            };
            var data = new Dictionary<string, object>
            {
                { "number", 23 },
                { "boolean", false },
                { "object", subData }
            };
            var jObject = JObject.FromObject(data);
            var jObject2 = JObject.FromObject(jObject);
            var jsonString = jObject2.ToString(Formatting.None);
            UT.Assert.AreEqual("{\"number\":23,\"boolean\":false,\"object\":{\"a\":12,\"b\":13}}", jsonString);
        }

        private class TestException : Exception
        {

        }
    }

    internal class SlowLogger : IAsyncLogger
    {
        private readonly TimeSpan _delay;

        public SlowLogger(TimeSpan delay)
        {
            _delay = delay;
        }

        public async Task LogAsync(LogRecord logRecord)
        {
            await Task.Delay(_delay);
            Console.Write($"{logRecord.ToLogString()} ");
        }
    }

    internal class RecursiveSyncLogger : ISyncLogger
    {
        public static string Message { get; private set; }
        public static bool HasFailed { get; private set; }
        public static bool IsRunning { get; set; }
        public static int InstanceCount { get; set; }
        private static readonly object ClassLock = new object();


        /// <inheritdoc />
        public void LogSync(LogRecord logRecord)
        {
            lock (ClassLock)
            {
                InstanceCount++;
                IsRunning = true;
            }

            var recursiveLogMessage = "Recursive log message";
            var recursive = logRecord.Message == recursiveLogMessage;
            if (recursive)
            {
                if (!HasFailed)
                {
                    HasFailed = true;
                    Message =
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        $"The {nameof(LogSync)}() method should never be called recursively. The Log class should have detected the recursion." +
                        " {nameof(recursive)} = {recursive}, {nameof(LogHelper.IsRecursiveLogging)} = {LogHelper.IsRecursiveLogging}";
                }
            }

            Console.WriteLine(logRecord.Message);
            // Try to provoke a recursive log call of this method
            if (!HasFailed) Log.LogError(recursiveLogMessage);
            lock (ClassLock)
            {
                InstanceCount--;
                if (InstanceCount < 0)
                {
                    if (!HasFailed)
                    {
                        HasFailed = true;
                        Message =
                            $"Unexpectedly had an {nameof(InstanceCount)} with value {InstanceCount} < 0";
                    }

                    InstanceCount = 0;
                }

                if (InstanceCount == 0) IsRunning = false;
            }
        }
    }
}
