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
            Assert.AreEqual(default, initialItem.Id);
            var id = await CrdStorage.CreateAsync(initialItem);
            var createdItem = await CrdStorage.ReadAsync(id);
            Assert.IsNotNull(createdItem);
            Assert.AreNotEqual(default, createdItem.Id);
            Assert.AreEqual(id, createdItem.Id);
            initialItem.Id = createdItem.Id;
            Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task CreateAndReturn_Id_Async()
        {
            var initialItem = new TestItemId<TId>();
            initialItem.InitializeWithDataForTesting(TypeOfTestDataEnum.Default);
            Assert.AreEqual(default, initialItem.Id);
            var createdItem = await CrdStorage.CreateAndReturnAsync(initialItem);
            Assert.IsNotNull(createdItem);
            Assert.AreNotEqual(default, createdItem.Id);
            initialItem.Id = createdItem.Id;
            Assert.AreEqual(initialItem, createdItem);
        }

        /// <summary>
        /// Create a lock.
        /// </summary>
        [TestMethod]
        public async Task ClaimLock()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
            Assert.AreEqual(id, itemLock.ItemId);
            Assert.IsTrue(itemLock.ValidUntil > DateTimeOffset.Now.AddSeconds(20));
        }

        /// <summary>
        /// Create a lock and then lock it again (which should fail).
        /// </summary>
        [TestMethod]
        public async Task ClaimLock_Given_AlreadyLocked_GivesException()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
            try
            {
                var itemLock2 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
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
        public async Task Claim_Release_Claim()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
            await CrdStorage.ReleaseDistributedLockAsync(itemLock1.ItemId, itemLock1.LockId);
            var itemLock2 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));;
            Assert.AreEqual(id, itemLock2.ItemId);
            Assert.AreNotEqual(itemLock1.LockId, itemLock2.LockId);
            Assert.IsTrue(itemLock2.ValidUntil > DateTimeOffset.UtcNow.AddSeconds(20));
        }

        /// <summary>
        /// Create a lock, release the lock and lock it again (which should succeed).
        /// </summary>
        [TestMethod]
        public async Task Claim_Reclaim()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
            await Task.Delay(10);
            var itemLock2 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30), itemLock1.LockId);
            Assert.AreEqual(id, itemLock2.ItemId);
            Assert.AreEqual(itemLock1.LockId, itemLock2.LockId);
            Assert.IsTrue(itemLock2.ValidUntil > itemLock1.ValidUntil);
        }
    }
}

