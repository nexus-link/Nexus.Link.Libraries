using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.SqlServer.Logic;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class SqlExtensionsTests
    {
        private Mock<IDbConnection> _connectionMock;

        [TestInitialize]
        public void Initialize()
        {
            SqlExtensions.ResetCache();
            _connectionMock = new Mock<IDbConnection>();
            _connectionMock.SetupProperty(x => x.ConnectionString);
            _connectionMock.Object.ConnectionString = "Server=localhost;Database=mock-database;";
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumResourceException))]
        public async Task Throws_ResourceException_When_Open_Connection_Throws_Exception()
        {
            _connectionMock.Setup(x => x.Open()).Throws(new ApplicationException("unavailable"));

            await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task Breaks_Circuit_On_Consecutive_Invocations()
        {
            var innerException = new ApplicationException("unavailable");
            _connectionMock
                .Setup(x => x.Open())
                .Callback(async () => await Task.Delay(200))
                .Throws(innerException)
                .Verifiable();

            // Fail once to set the fail state
            try
            {
                await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
                Assert.Fail("Verify should throw (1)");
            }
            catch (FulcrumResourceException e)
            {
                _connectionMock.Verify(x => x.Open(), Times.Once);
                Assert.AreEqual(innerException, e.InnerException);
            }

            // Load test
            var resetEvent = new ManualResetEvent(false);
            var count = 0;
            const int parallelCalls = 1000;

            Parallel.For(0, parallelCalls, async i =>
            {
                try
                {
                    await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
                    Assert.Fail("Verify should throw (2)");
                }
                catch (FulcrumResourceException)
                {
                    _connectionMock.Verify(x => x.Open(), Times.Once);
                    if (Interlocked.Increment(ref count) >= parallelCalls) resetEvent.Set();
                }
            });

            Assert.IsTrue(resetEvent.WaitOne(TimeSpan.FromMilliseconds(300)), "A small time span should be enough to wait for all calls");
            Assert.AreEqual(parallelCalls, count);
        }

        [TestMethod]
        public async Task Recovers_After_Success()
        {

        }
    }
}
