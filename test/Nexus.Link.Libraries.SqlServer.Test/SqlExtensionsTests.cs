using System;
using System.Data;
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
            // TODO: Paralell.Loop ...
            var innerException = new ApplicationException("unavailable");
            _connectionMock.Setup(x => x.Open()).Throws(innerException).Verifiable();

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
            try
            {
                await _connectionMock.Object.VerifyAvailabilityAsync(TimeSpan.FromSeconds(1));
                Assert.Fail("Verify should throw (2)");
            }
            catch (FulcrumResourceException e2)
            {
                _connectionMock.Verify(x => x.Open(), Times.Once);
                Assert.AreNotEqual(innerException, e2.InnerException);
            }
        }

        [TestMethod]
        public async Task Recovers_After_Success()
        {

        }
    }
}
