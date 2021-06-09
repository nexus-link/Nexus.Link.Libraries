using System;
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
    public abstract class TestICrdId<TId> : TestICrdBase<TestItemBare, TestItemId<TId>, TId>
    { 
        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task Create_Read_Id_Async()
        {
            var initialItem = new TestItemId<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(default, initialItem.Id);
            var id = await CrdStorage.CreateAsync(initialItem);
            var createdItem = await CrdStorage.ReadAsync(id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(createdItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(default, createdItem.Id);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(id, createdItem.Id);
            initialItem.Id = createdItem.Id;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task CreateAndReturn_Id_Async()
        {
            var initialItem = new TestItemId<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(default, initialItem.Id);
            var createdItem = await CrdStorage.CreateAndReturnAsync(initialItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(createdItem);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(default, createdItem.Id);
            initialItem.Id = createdItem.Id;
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create a lock.
        /// </summary>
        [TestMethod]
        public async Task Lock_Async()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock = await CrdStorage.ClaimLockAsync(id);
            Assert.AreEqual(id, itemLock.ItemId);
            Assert.IsTrue(itemLock.ValidUntil > DateTimeOffset.Now.AddSeconds(20));
        }

        /// <summary>
        /// Create a lock and then lock it again (which should fail).
        /// </summary>
        [TestMethod]
        public async Task LockFailAsync()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimLockAsync(id);
            try
            {
                var itemLock2 = await CrdStorage.ClaimLockAsync(id);
                Assert.Fail($"Expected an exception of type {nameof(FulcrumTryAgainException)}.");
            }
            catch (FulcrumTryAgainException ex)
            {
                Assert.IsTrue(ex.RecommendedWaitTimeInSeconds <= 30, $"{nameof(ex.RecommendedWaitTimeInSeconds)}: {ex.RecommendedWaitTimeInSeconds}");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected an exception of type {nameof(FulcrumTryAgainException)}, byt received exception of type {ex.GetType().FullName}.");
            }
        }

        /// <summary>
        /// Create a lock, release the lock and lock it again (which should succeed).
        /// </summary>
        [TestMethod]
        public async Task LockReleaseLockAsync()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimLockAsync(id);
            await CrdStorage.ReleaseLockAsync(id, itemLock1.Id);
            var itemLock2 = await CrdStorage.ClaimLockAsync(id);
            Assert.AreEqual(id, itemLock2.ItemId);
            Assert.AreNotEqual(itemLock1.Id, itemLock2.Id);
            Assert.IsTrue(itemLock2.ValidUntil > DateTimeOffset.Now.AddSeconds(20));
        }
    }
}

