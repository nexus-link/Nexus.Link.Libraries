using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Test.NuGet.Crud;
using Nexus.Link.Libraries.Crud.Test.NuGet.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class CrudTestValidated : TestICrudValidated<Guid>
    {
        private CrudSql<TestItemBare, TestItemValidated<Guid>> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTestValidated));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            _storage = new CrudSql<TestItemBare, TestItemValidated<Guid>>(connectionString, tableMetadata);
        }

        protected override ICrud<TestItemBare, TestItemValidated<Guid>, Guid> CrudStorage => _storage;
    }
}