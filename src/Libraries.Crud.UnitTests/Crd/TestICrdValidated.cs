using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Crd
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestICrdValidated<TId> : TestICrdBase<TestItemBare, TestItemValidated<TId>, TId>
    {
        /// <summary>
        /// Try to create an item that is not valid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public async Task Create_ValidationFailed_Async()
        {
            var initialItem = new TestItemValidated<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.ValidationFail);
            await CrdStorage.CreateAsync(initialItem);
            Assert.Fail($"Expected the method {nameof(CrdStorage.CreateAsync)} to detect that the data was not valid and throw the exception {nameof(FulcrumContractException)}.");
        }

        /// <summary>
        /// Create a bare item
        /// </summary>
        [TestMethod]
        public async Task Create_Read_Validated_Async()
        {
            var initialItem = new TestItemValidated<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            Assert.AreEqual(default, initialItem.Id);
            initialItem.Validate(null);
            var id = await CrdStorage.CreateAsync(initialItem);
            var createdItem = await CrdStorage.ReadAsync(id);
            Assert.IsNotNull(createdItem);
            Assert.AreNotEqual(createdItem.Id, default);
            createdItem.Validate(null);
            initialItem.Id = createdItem.Id;
            Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Try to create an item that is not valid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public async Task CreateAndReturn_ValidationFailed_Async()
        {
            var initialItem = new TestItemValidated<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.ValidationFail);
            await CrdStorage.CreateAndReturnAsync(initialItem);
            Assert.Fail($"Expected the method {nameof(CrdStorage.CreateAndReturnAsync)} to detect that the data was not valid and throw the exception {nameof(FulcrumContractException)}.");
        }

        /// <summary>
        /// Create a bare item
        /// </summary>
        [TestMethod]
        public async Task CreateAndReturn_Validated_Async()
        {
            var initialItem = new TestItemValidated<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            Assert.AreEqual(default, initialItem.Id);
            initialItem.Validate(null);
            var createdItem = await CrdStorage.CreateAndReturnAsync(initialItem);
            Assert.IsNotNull(createdItem);
            Assert.AreNotEqual(createdItem.Id, default);
            createdItem.Validate(null);
            initialItem.Id = createdItem.Id;
            Assert.AreEqual(initialItem, createdItem);
        }
    }
}

