using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.ManyToOne;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class ManyToOneTest : TestIManyToOne<Guid, Guid?>
    {
        private CrudSql<TestItemId<Guid>> _oneStorage;
        private ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid> _manyStorage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ManyToOneTest));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var manyTableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value", "ParentId" },
                OrderBy = new string[] { }
            };
            var oneTableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            _oneStorage = new CrudSql<TestItemId<Guid>>(connectionString, oneTableMetadata);
            _manyStorage = new ManyToOneSql<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, TestItemId<Guid>>(connectionString, manyTableMetadata, "ParentId", _oneStorage);
        }

        /// <inheritdoc />
        protected override ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid>
            CrudManyStorageRecursive => null;

        /// <inheritdoc />
        protected override ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid>
            CrudManyStorageNonRecursive => _manyStorage;

        /// <inheritdoc />
        protected override ICrud<TestItemId<Guid>, Guid> OneStorage => _oneStorage;
    }
}
