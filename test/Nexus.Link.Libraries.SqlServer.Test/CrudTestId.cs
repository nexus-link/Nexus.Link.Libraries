using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class CrudTestId : TestICrudId<Guid>
    {
        private CrudSql<TestItemBare, TestItemId<Guid>> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTestId));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] {}
            };
            _storage = new CrudSql<TestItemBare, TestItemId<Guid>>(connectionString, tableMetadata);
        }

        protected override ICrud<TestItemBare, TestItemId<Guid>, Guid> CrudStorage => _storage;
    }
}