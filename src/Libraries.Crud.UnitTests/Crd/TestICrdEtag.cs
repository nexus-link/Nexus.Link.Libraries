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
    public abstract class TestICrdEtag<TId> : TestICrdBase<TestItemBare, TestItemEtag<TId>, TId>
    {
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
            Assert.AreNotEqual(createdItem.Id, default);
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
            Assert.AreNotEqual(createdItem.Id, default);
            Assert.IsNotNull(createdItem.Etag);
            Assert.AreEqual(initialItem, createdItem);
        }
    }
}

