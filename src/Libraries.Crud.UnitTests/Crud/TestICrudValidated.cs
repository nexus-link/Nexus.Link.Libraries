using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Crud
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestICrudValidated<TId> : TestICrdValidated<TId>
    {
        protected override ICrud<TestItemBare, TestItemValidated<TId>, TId> CrdStorage => CrudStorage;

        /// <summary>
        /// Try to create an item that is not valid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public async Task Update_ValidationFailed_Async()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Default);
            Assert.AreNotEqual(default, id);
            var updatedItem = new TestItemValidated<TId>();
            updatedItem.InitializeWithDataForTesting(TypeOfTestDataEnum.ValidationFail);
            await CrudStorage.UpdateAsync(id, updatedItem);
            Assert.Fail($"Expected the method {nameof(CrudStorage.UpdateAsync)} to detect that the data was not valid and throw the exception {nameof(FulcrumContractException)}.");
        }

        /// <summary>
        /// Create a bare item
        /// </summary>
        [TestMethod]
        public async Task Update_Read_Validated_Async()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var updatedItem = await UpdateItemAsync(id, TypeOfTestDataEnum.Variant2);
            var readItem = await ReadItemAsync(id);
            readItem.Validate(null);
            Assert.AreEqual(updatedItem, readItem);
        }
    }
}

