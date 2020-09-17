using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Logging
{
    [TestClass]
    public class TestLogHelper
    {
        private LogSeverityLevel _loggedSeverityLevel;
        private string _loggedMessage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestLogHelper).FullName);
            _loggedSeverityLevel = LogSeverityLevel.None;
            _loggedMessage = null;
            var fallbackLoggerMock = new Mock<IFallbackLogger>();
            fallbackLoggerMock
                .Setup(logger => logger.SafeLog(It.IsAny<LogSeverityLevel>(), It.IsAny<string>()))
                .Callback((LogSeverityLevel sl, string m) =>
                {
                    _loggedSeverityLevel = sl;
                    _loggedMessage = m;
                });
            FulcrumApplication.Setup.FallbackLogger = fallbackLoggerMock.Object;
        }

        [TestMethod]
        public void FallbackSafeLog()
        {
            const LogSeverityLevel expectedLevel = LogSeverityLevel.Error;
            const string exceptionMessage = "ExceptionMessage";
            const string message = "TestMessage";
            try
            {
                throw new TestException(exceptionMessage);
            }
            catch (Exception expectedException)
            {
                // ReSharper disable ExplicitCallerInfoArgument
                LogHelper.FallbackSafeLog(expectedLevel, message, expectedException, "memberName", "filePath", 42);
                // ReSharper restore ExplicitCallerInfoArgument
                UT.Assert.AreEqual(expectedLevel, _loggedSeverityLevel);
                UT.Assert.IsNotNull(_loggedMessage);
                // ReSharper disable ExplicitCallerInfoArgument
                UT.Assert.IsTrue(_loggedMessage.Contains(LogHelper.LocationToLogString("memberName", "filePath", 42)));
                // ReSharper restore ExplicitCallerInfoArgument
                UT.Assert.IsTrue(_loggedMessage.Contains(FulcrumApplication.ToLogString()));
            }
        }

        [TestMethod]
        public void FallbackSafeLogCallingClient()
        {
            var expectedString = Guid.NewGuid().ToString();
            FulcrumApplication.Context.CallingClientName = expectedString;
            LogHelper.FallbackSafeLog(LogSeverityLevel.Error, "test");
            UT.Assert.IsNotNull(_loggedMessage);
            UT.Assert.IsTrue(_loggedMessage.Contains(expectedString));
        }

        [TestMethod]
        public void FallbackSafeLogCorrelationId()
        {
            var expectedString = Guid.NewGuid().ToString();
            FulcrumApplication.Context.CorrelationId = expectedString;
            LogHelper.FallbackSafeLog(LogSeverityLevel.Error, "test");
            UT.Assert.IsNotNull(_loggedMessage);
            UT.Assert.IsTrue(_loggedMessage.Contains(expectedString));
        }

        [TestMethod]
        public void FallbackSafeLogTenant()
        {
            var org = Guid.NewGuid().ToString();
            var env = Guid.NewGuid().ToString();
            var expectedTenant = new Tenant(org, env);
            FulcrumApplication.Context.ClientTenant = expectedTenant;
            LogHelper.FallbackSafeLog(LogSeverityLevel.Error, "test");
            UT.Assert.IsNotNull(_loggedMessage);
            UT.Assert.IsTrue(_loggedMessage.Contains(expectedTenant.ToLogString()));
        }

        private class TestException : Exception
        {
            public TestException(string message) : base(message)
            {
            }
        }
    }
}
