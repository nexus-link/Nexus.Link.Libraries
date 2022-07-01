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
    public class CrudTestEtag : TestICrudEtag<Guid>
    {
        private CrudSql<TestItemBare, TestItemEtag<Guid>> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTestEtag));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                EtagColumnName = "Etag",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            _storage = new CrudSql<TestItemBare, TestItemEtag<Guid>>(new DatabaseOptions
            {
                ConnectionString = connectionString
            }, tableMetadata);
        }

        protected override ICrud<TestItemBare, TestItemEtag<Guid>, Guid> CrudStorage => _storage;
    }
}