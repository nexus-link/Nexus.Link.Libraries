using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Misc.Models;
using Nexus.Link.Libraries.SqlServer.Logic;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class SqlExtensionsWithRealCircuitBreakerTests
    {
        private Mock<IDbConnection> _connectionMock;

        [TestInitialize]
        public void Initialize()
        {
            SqlExtensions.CircuitBreakerCollection.ResetCollection();
            _connectionMock = new Mock<IDbConnection>();
            _connectionMock.SetupProperty(x => x.ConnectionString);
            _connectionMock.Object.ConnectionString = "Server=localhost;Database=mock-database;";
            SqlExtensions.CircuitBreakerCollection = new CircuitBreakerCollection(() => new CircuitBreaker(new CircuitBreakerOptions
            {
                CancelConcurrentRequestsWhenOneFails = false,
                CoolDownStrategy = new CoolDownStrategy(TimeSpan.FromSeconds(1))
            }));
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumResourceException))]
        public async Task Throws_ResourceException_When_Open_Connection_Throws_Exception_Async()
        {
            _connectionMock.Setup(x => x.Open()).Throws(new ApplicationException("unavailable"));

            await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumResourceException))]
        public void Throws_ResourceException_When_Open_Connection_Throws_Exception_Sync()
        {
            _connectionMock.Setup(x => x.Open()).Throws(new ApplicationException("unavailable"));

            _connectionMock.Object.VerifyAvailability(TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        public async Task Breaks_Circuit_On_Consecutive_Invocations_Async()
        {
            var innerException = new ApplicationException("unavailable");
            _connectionMock
                .Setup(x => x.Open())
                .Callback(() => Task.Delay(200).Wait())
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

            Assert.IsTrue(resetEvent.WaitOne(TimeSpan.FromSeconds(1)), "A small time span should be enough to wait for all calls");
            Assert.AreEqual(parallelCalls, count);
        }

        [TestMethod]
        public void Breaks_Circuit_On_Consecutive_Invocations_Sync()
        {
            var innerException = new ApplicationException("unavailable");
            _connectionMock
                .Setup(x => x.Open())
                .Callback(() => Task.Delay(200).Wait())
                .Throws(innerException)
                .Verifiable();

            // Fail once to set the fail state
            try
            {
                _connectionMock.Object.VerifyAvailability(TimeSpan.FromSeconds(1));
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

            Parallel.For(0, parallelCalls, i =>
            {
                try
                {
                    _connectionMock.Object.VerifyAvailability(TimeSpan.FromSeconds(1));
                    Assert.Fail("Verify should throw (2)");
                }
                catch (FulcrumResourceException)
                {
                    _connectionMock.Verify(x => x.Open(), Times.Once);
                    if (Interlocked.Increment(ref count) >= parallelCalls) resetEvent.Set();
                }
            });

            Assert.IsTrue(resetEvent.WaitOne(TimeSpan.FromSeconds(1)), "A small time span should be enough to wait for all calls");
            Assert.AreEqual(parallelCalls, count);
        }

        [TestMethod]
        public async Task Recovers_After_Success_Async()
        {
            // 1. Fail
            _connectionMock.Setup(x => x.Open()).Throws(new ApplicationException("unavailable"));
            try
            {
                await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
                Assert.Fail("Verify should throw");
            }
            catch (FulcrumResourceException)
            {
                _connectionMock.Verify(x => x.Open(), Times.Once, "Open() should have been executed");
            }

            // 2. Verify failed state
            try
            {
                await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
                Assert.Fail("Verify should throw");
            }
            catch (FulcrumResourceException)
            {
                _connectionMock.Verify(x => x.Open(), Times.Once, "Open() should NOT have been executed");
            }

            // 3. Recover
            SqlExtensions.CircuitBreakerCollection.ForceEndOfCoolDown();
            _connectionMock.Setup(x => x.Open()).Verifiable();

            await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
            _connectionMock.Verify(x => x.Open(), Times.Exactly(2), "Open() should have been executed again after recovery");
        }

        [TestMethod]
        public void Recovers_After_Success_Sync()
        {
            // 1. Fail
            _connectionMock.Setup(x => x.Open()).Throws(new ApplicationException("unavailable"));
            try
            {
                _connectionMock.Object.VerifyAvailability(TimeSpan.FromSeconds(1));
                Assert.Fail("Verify should throw");
            }
            catch (FulcrumResourceException)
            {
                _connectionMock.Verify(x => x.Open(), Times.Once, "Open() should have been executed");
            }

            // 2. Verify failed state
            try
            {
                _connectionMock.Object.VerifyAvailability(TimeSpan.FromSeconds(1));
                Assert.Fail("Verify should throw");
            }
            catch (FulcrumResourceException)
            {
                _connectionMock.Verify(x => x.Open(), Times.Once, "Open() should NOT have been executed");
            }

            // 3. Recover
            SqlExtensions.CircuitBreakerCollection.ForceEndOfCoolDown();
            _connectionMock.Setup(x => x.Open()).Verifiable();

            _connectionMock.Object.VerifyAvailability(TimeSpan.FromSeconds(1));
            _connectionMock.Verify(x => x.Open(), Times.Exactly(2), "Open() should have been executed again after recovery");
        }
    }
}
