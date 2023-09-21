using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class CrudSqlTests
    {
        private ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> _storage;

        [TestInitialize]
        public async Task Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(this.GetType().Name);
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value", "IncreasingNumber", "NumberModulo", "DecreasingString" },
                OrderBy = new string[] { }
            };
            _storage = new CrudSql<TestItemSort<Guid>, TestItemSort<Guid>>(new DatabaseOptions
            {
                ConnectionString = connectionString
            }, tableMetadata);
            await _storage.DeleteAllAsync();
        }

        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task Search_DefaultSortOrder_Async()
        {
            // Arrange
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value", "IncreasingNumber", "NumberModulo", "DecreasingString" },
                OrderBy = new string[] { "RecordCreatedAt"}
            };
            var tableMetadataMock = new Mock<ISqlTableMetadata>();
            _storage = new CrudSql<TestItemSort<Guid>, TestItemSort<Guid>>(new DatabaseOptions
            {
                ConnectionString = connectionString
            }, tableMetadataMock.Object);
            var searchDetails = new SearchDetails<TestItemSort<Guid>>(new {Id = Guid.NewGuid()}, new {});

            tableMetadataMock
                .SetupGet(md => md.TableName)
                .Returns(tableMetadata.TableName);
            tableMetadataMock
                .SetupGet(md => md.CustomColumnNames)
                .Returns(tableMetadata.CustomColumnNames);
            tableMetadataMock
                .Setup(md => md.GetOrderBy(It.IsAny<string>()))
                .Returns(tableMetadata.GetOrderBy())
                .Verifiable();
            await _storage.DeleteAllAsync();


            // Act
            await _storage.SearchAsync(searchDetails, 0, 10);

            // Assert
            tableMetadataMock.Verify();
        }
    }
}
