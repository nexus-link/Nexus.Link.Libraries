using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.SqlServer.Logic;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class DatabaseTests
    {
        private Database _database;
        private Mock<IDatabaseOptions> _optionsMock;

        [TestInitialize]
        public void Initialize()
        {
            _optionsMock = new Mock<IDatabaseOptions>();
            _optionsMock
                .SetupGet(x => x.ConnectionString)
                .Returns(TestSettings.ConnectionString)
                .Verifiable();
            
            _database = new Database(_optionsMock.Object);
        }

        [TestMethod]
        public async Task Can_Interact_Before_New_Connection()
        {
            // Arrange
            _optionsMock
                .Setup(x => x.OnBeforeNewSqlConnectionAsync)
                .Returns((connectionString, token) => Task.CompletedTask)
                .Verifiable();

            // Act
            await _database.NewSqlConnectionAsync();

            // Assert
            _optionsMock.Verify();
        }
    }
}
