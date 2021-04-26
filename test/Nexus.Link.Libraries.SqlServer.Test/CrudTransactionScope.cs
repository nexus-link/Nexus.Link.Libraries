using System;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.SqlServer.Model;
using Nexus.Link.Libraries.SqlServer.Test.Stuff;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class CrudTransactionScope
    {
        private Storage _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTransactionScope));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);

            var trans1TableMetadata = new SqlTableMetadata
            {
                TableName = "TestTrans1",
                CustomColumnNames = new[] { "Field1", "Field2" },
                OrderBy = new string[] { }
            };

            var trans2TableMetadata = new SqlTableMetadata
            {
                TableName = "TestTrans2",
                CustomColumnNames = new[] { "Field1", "Field2" },
                OrderBy = new string[] { }
            };

            var trans1 = new CrudSql<TestTrans1Create, TestTrans1>(connectionString, trans1TableMetadata);
            var trans2 = new CrudSql<TestTrans2Create, TestTrans2>(connectionString, trans2TableMetadata);

            _storage = new Storage(trans1, trans2);
        }

        [TestMethod]
        public async Task CreateAsync_WithTransaction_Succeeds()
        {
            Guid trans1Id = Guid.Empty;
            Guid trans2Id = Guid.Empty;

            using (var scope = new TransactionScope())
            {
                //Act
                trans1Id = await _storage.TestTrans1.CreateAsync(new TestTrans1Create());
                trans2Id = await _storage.TestTrans2.CreateAsync(new TestTrans2Create());
                scope.Complete();

            }

            //Assert
            var trans1 = await _storage.TestTrans1.ReadAsync(trans1Id);
            Assert.IsNotNull(trans1, $"Expected trans1 item not to be null");
            var trans2 = await _storage.TestTrans2.ReadAsync(trans2Id);
            Assert.IsNotNull(trans2, $"Expected trans2 item not to be null");
        }
    }
}