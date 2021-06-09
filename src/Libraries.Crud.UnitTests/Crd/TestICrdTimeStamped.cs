using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Crd
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestICrdTimeStamped<TId> : TestICrdBase<TestItemBare, TestItemTimestamped<TId>, TId>
    {
        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task Create_Read_Async()
        {
            var initialItem = new TestItemTimestamped<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(default, initialItem.Id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(initialItem.RecordCreatedAt == default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(initialItem.RecordUpdatedAt == default);
            var id = await CrdStorage.CreateAsync(initialItem);
            var createdItem = await CrdStorage.ReadAsync(id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(createdItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(createdItem.Id, default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(createdItem.RecordCreatedAt != default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(createdItem.RecordUpdatedAt != default);
            initialItem.Id = createdItem.Id;
            initialItem.RecordCreatedAt = createdItem.RecordCreatedAt;
            initialItem.RecordUpdatedAt = createdItem.RecordUpdatedAt;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create an item with an etag.
        /// </summary>
        [TestMethod]
        public async Task CreateAndReturn_Read_Async()
        {
            var initialItem = new TestItemTimestamped<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(default, initialItem.Id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(initialItem.RecordCreatedAt == default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(initialItem.RecordUpdatedAt == default);
            var createdItem = await CrdStorage.CreateAndReturnAsync(initialItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(createdItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(createdItem.Id, default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(createdItem.RecordCreatedAt != default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(createdItem.RecordUpdatedAt != default);
            initialItem.Id = createdItem.Id;
            initialItem.RecordCreatedAt = createdItem.RecordCreatedAt;
            initialItem.RecordUpdatedAt = createdItem.RecordUpdatedAt;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(initialItem, createdItem);
        }
    }
}

