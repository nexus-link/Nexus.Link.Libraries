using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Test.NuGet.Model;

namespace Nexus.Link.Libraries.Crud.Test.NuGet.Crd
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestICrdBare<TId> : TestICrdBase<TestItemBare, TestItemBare, TId>
    {

        /// <summary>
        /// Create a bare item
        /// </summary>
        [TestMethod]
        public async Task Create_Read_Async()
        {
            var initialItem = new TestItemBare();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            var id = await CrdStorage.CreateAsync(initialItem);
            var createdItem = await CrdStorage.ReadAsync(id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(createdItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create a bare item
        /// </summary>
        [TestMethod]
        public async Task CreateAndReturn_Async()
        {
            var initialItem = new TestItemBare();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            var createdItem = await CrdStorage.CreateAndReturnAsync(initialItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(createdItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Try to read an item that doesn't exist yet.
        /// </summary>
        [TestMethod]
        public async Task Read_NotFound_Async()
        {
            var item = await CrdStorage.ReadAsync(CrudHelper.CreateNewId<TId>());
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(item);
        }

        /// <summary>
        /// Delete an item
        /// </summary>
        [TestMethod]
        public async Task Delete_Async()
        {
            var initialItem = new TestItemBare();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            var id = await CrdStorage.CreateAsync(initialItem);
            await CrdStorage.ReadAsync(id);
            await CrdStorage.DeleteAsync(id);
            var item = await CrdStorage.ReadAsync(id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(item);
        }

        /// <summary>
        /// Try to read an item that doesn't exist. Should not result in an exception.
        /// </summary>
        [TestMethod]
        public async Task Delete_NotFound()
        {
            await CrdStorage.DeleteAsync(CrudHelper.CreateNewId<TId>());
        }
    }
}

