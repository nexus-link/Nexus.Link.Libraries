using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    /// <summary>
    /// Tests for testing RecordVersion implementation <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public class CrudTestRecordVersion : TestICrdBase<TestItemBare, TestRecordVersion, Guid>
    {
        private CrudSql<TestItemBare, TestRecordVersion> _storage;

        /// <inheritdoc />
        protected override ICrud<TestItemBare, TestRecordVersion, Guid> CrdStorage  => _storage;

        /// <inheritdoc />
        protected override ICrud<TestItemBare, TestRecordVersion, Guid> CrudStorage => _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTestEtag));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                RowVersionColumnName = "RecordVersion",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            _storage = new CrudSql<TestItemBare, TestRecordVersion>(connectionString, tableMetadata);
        }

        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task Create_Read_Etag_Async()
        {
            var initialItem = new TestItemBare();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            var id = await CrdStorage.CreateAsync(initialItem);
            var createdItem = await CrdStorage.ReadAsync(id);
            Assert.IsNotNull(createdItem);
            Assert.AreNotEqual(createdItem.Id, Guid.Empty);
            Assert.IsNotNull(createdItem.Etag);
            Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create an item with an etag.
        /// </summary>
        [TestMethod]
        public async Task CreateAndReturn_Read_Etag_Async()
        {
            var initialItem = new TestItemBare();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            var createdItem = await CrdStorage.CreateAndReturnAsync(initialItem);
            Assert.IsNotNull(createdItem);
            Assert.AreNotEqual(createdItem.Id, Guid.Empty);
            Assert.IsNotNull(createdItem.Etag);
            Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task Update_Read_Etag_Async()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var updateItem = await UpdateItemAsync(id, TypeOfTestDataEnum.Variant2);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(updateItem);
            var readItem = await ReadItemAsync(id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(readItem);
            if (!updateItem.Equals(readItem))
            {
                updateItem.Etag = readItem.Etag;
            }
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(updateItem, readItem);
        }
    }

    public class TestRecordVersion : TestItemEtag<Guid>, IRecordVersion
    {
        /// <inheritdoc />
        public byte[] RecordVersion { get; set; }
    }
}

