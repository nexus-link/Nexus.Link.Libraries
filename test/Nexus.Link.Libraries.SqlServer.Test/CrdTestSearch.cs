using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class CrdTestSearch : TestICrdSearch<Guid>
    {
        private ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> _storage;

        [TestInitialize]
        public async Task Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrdTestSearch));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value", "IncreasingNumber", "NumberModulo", "DecreasingString" },
                OrderBy = new string[] {}
            };
            _storage = new CrudSql<TestItemSort<Guid>, TestItemSort<Guid>>(connectionString, tableMetadata);
            await _storage.DeleteAllAsync();
        }

        protected override ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> CrdStorage => _storage;

        protected override ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> CrudStorage => _storage;
    }
}
