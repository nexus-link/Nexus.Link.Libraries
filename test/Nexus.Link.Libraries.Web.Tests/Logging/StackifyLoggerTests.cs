using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Logging.Stackify;

namespace Nexus.Link.Libraries.Web.Tests.Logging
{
    [TestClass]
    public class StackifyLoggerTests
    {
        private StackifyLogger _logger;

        [TestInitialize]
        public void RunBeforeEachTestCase()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(StackifyLoggerTests).FullName);
            _logger = new StackifyLogger(null);
        }

        [TestMethod]
        public async Task LogAsyncAsync()
        {
            var logRecord = new LogRecord
            {
                Message = Guid.NewGuid().ToString(),
                Location = Guid.NewGuid().ToString(),
                SeverityLevel = LogSeverityLevel.Critical,
                TimeStamp = DateTimeOffset.Now
            };
            await _logger.LogAsync(logRecord);
            Assert.IsNotNull(logRecord);
            Assert.IsTrue(_logger.LastSentEnvelope.Contains(logRecord.Message));
            Assert.IsTrue(_logger.LastSentEnvelope.Contains(logRecord.Location));
            Assert.IsTrue(_logger.LastSentEnvelope.Contains(logRecord.SeverityLevel.ToString()));
            Assert.IsTrue(_logger.LastSentEnvelope.Contains(logRecord.TimeStamp.ToUnixTimeMilliseconds().ToString()));
        }
    }
}
