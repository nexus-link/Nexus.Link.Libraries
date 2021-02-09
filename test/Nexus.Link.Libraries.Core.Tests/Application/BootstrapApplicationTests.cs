using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Application
{
    [TestClass]
    public class BootstrapApplicationTests
    {
        [TestMethod]
        public void RunTimeLevel_Cant_Be_None()
        {
            try
            {
                // Before bug fix 4.20.1 this would through exception due to Log.LogError doing validation on ApplicationSetup,
                // which had no chance of setting the ApplicationSetup properties because the logging intervened
                FulcrumApplicationHelper.RuntimeSetup("app-name", new Tenant("o", "e"), RunTimeLevelEnum.None);
                UT.Assert.Fail("Expected exception");
            }
            catch (Exception e)
            {
                UT.Assert.IsTrue(e.Message.Contains(nameof(RunTimeLevelEnum.None)),
                    $"Expected error message to contain {nameof(RunTimeLevelEnum.None)}, but it didn't. Was {e.Message}");
            }
        }

        /// <summary>
        /// The 4.20.1 fix changed GenericBase. Make sure it can still LogError when it's supposed to.
        /// </summary>
        [TestMethod]
        public void GenericBase_Can_Still_Log_Error()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(BootstrapApplicationTests));

            var loggerMock = new Mock<ISyncLogger>();
            FulcrumApplication.Setup.SynchronousFastLogger = loggerMock.Object;

            LogRecord loggedRecord = null;
            loggerMock
                .Setup(x => x.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord logRecord) =>
                {
                    loggedRecord = logRecord;
                    Console.WriteLine("LOGGED MESSAGE: " + logRecord.ToLogString());
                });

            try
            {
                FulcrumAssert.IsNotNull(null, "NIL");
                UT.Assert.Fail("Expected exception");
            }
            catch (Exception)
            {
                UT.Assert.IsNotNull(loggedRecord, "Expected a log record");
                UT.Assert.AreEqual(LogSeverityLevel.Error, loggedRecord.SeverityLevel);
            }
        }
    }
}
