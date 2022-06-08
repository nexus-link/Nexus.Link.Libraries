using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

#if NETSTANDARD
using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;
#endif

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

        #region DistributedLock

        /// <summary>
        /// Create a lock.
        /// </summary>
        [TestMethod]
        public async Task ClaimDistributedLock()
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
        public async Task ClaimDistributedLock_Given_AlreadyLocked_GivesException()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
            try
            {
                var itemLock2 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
                Assert.Fail($"Expected an exception of type {nameof(FulcrumResourceLockedException)}.");
            }
            catch (FulcrumResourceLockedException ex)
            {
                Assert.IsTrue(ex.RecommendedWaitTimeInSeconds <= 30, $"{nameof(ex.RecommendedWaitTimeInSeconds)}: {ex.RecommendedWaitTimeInSeconds}");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected an exception of type {nameof(FulcrumResourceLockedException)}, byt received exception of type {ex.GetType().FullName}.");
            }
        }

        /// <summary>
        /// Create a lock, release the lock and lock it again (which should succeed).
        /// </summary>
        [TestMethod]
        public async Task ClaimDistributedLock_Release_Claim()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
            await CrdStorage.ReleaseDistributedLockAsync(itemLock1.ItemId, itemLock1.LockId);
            var itemLock2 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30)); ;
            Assert.AreEqual(id, itemLock2.ItemId);
            Assert.AreNotEqual(itemLock1.LockId, itemLock2.LockId);
            Assert.IsTrue(itemLock2.ValidUntil > DateTimeOffset.UtcNow.AddSeconds(20));
        }

        /// <summary>
        /// Create a lock, release the lock and lock it again (which should succeed).
        /// </summary>
        [TestMethod]
        public async Task ClaimDistributedLock_Reclaim()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var itemLock1 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30));
            await Task.Delay(10);
            var itemLock2 = await CrdStorage.ClaimDistributedLockAsync(id, TimeSpan.FromSeconds(30), itemLock1.LockId);
            Assert.AreEqual(id, itemLock2.ItemId);
            Assert.AreEqual(itemLock1.LockId, itemLock2.LockId);
            Assert.IsTrue(itemLock2.ValidUntil > itemLock1.ValidUntil);
        }
        #endregion

        #region TransactionLock
#if NETSTANDARD

        /// <summary>
        /// Create a lock.
        /// </summary>
        [TestMethod]
        public async Task ClaimTransactionLock()
        {
            TId id = default;
            using (var scope = CreateNormalScope())
            {
                id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
                scope.Complete();
            }

            using (var scope = CreateNormalScope())
            {
                var lockedItem = await CrdStorage.ClaimTransactionLockAndReadAsync(id);
                Assert.IsNotNull(lockedItem);
                scope.Complete();
            }
        }

        /// <summary>
        /// Create a lock.
        /// </summary>
        [TestMethod]
        public async Task ClaimTransactionLock_Given_UnknownId_Gives_Null()
        {
            using (var scope = CreateNormalScope())
            {
                var lockedItem = await CrdStorage.ClaimTransactionLockAndReadAsync(StorageHelper.CreateNewId<TId>());
                Assert.IsNull(lockedItem);
                scope.Complete();
            }
        }

        /// <summary>
        /// Create a lock.
        /// </summary>
        [TestMethod]
        public async Task ClaimTransactionLock_Given_TwoConsecutive_Gives_Ok()
        {
            TId id = default;
            using (var scope = CreateNormalScope())
            {
                id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
                scope.Complete();
            }

            using (var scope = CreateNormalScope())
            {
                var lockedItem = await CrdStorage.ClaimTransactionLockAndReadAsync(id);
                Assert.IsNotNull(lockedItem);
                lockedItem = await CrdStorage.ClaimTransactionLockAndReadAsync(id);
                Assert.IsNotNull(lockedItem);
                scope.Complete();
            }
        }

        /// <summary>
        /// Create a lock and then lock it again (which should fail).
        /// </summary>
        [TestMethod]
        public async Task ClaimTransactionLock_Given_LockedAndRelease_Gives_NewLockIsOk()
        {
            TId id = default;
            using (var scope = CreateNormalScope())
            {
                id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
                scope.Complete();
            }

            var reallyLockedHandler = new ManualResetEvent(false);
            var tryLockHandler = new ManualResetEvent(false);
            bool? couldLock = null;
            var thread1 = ThreadHelper.FireAndForget(async () =>
            {
                using (var scope = CreateNormalScope())
                {
                    try
                    {
                        var lockedItem = await CrdStorage.ClaimTransactionLockAndReadAsync(id);
                        Assert.IsNotNull(lockedItem);
                        reallyLockedHandler.Set();
                        tryLockHandler.WaitOne(TimeSpan.FromSeconds(1));
                        scope.Complete();
                        couldLock = true;
                    }
                    catch (FulcrumResourceLockedException)
                    {
                        couldLock = false;
                    }
                }
            });
            var thread2 = ThreadHelper.FireAndForget(async () =>
            {
                using (var scope = CreateNormalScope())
                {
                    try
                    {
                        reallyLockedHandler.WaitOne(TimeSpan.FromSeconds(1));
                        var lockedItem2 = await CrdStorage.ClaimTransactionLockAndReadAsync(id);
                        Assert.IsNotNull(lockedItem2);
                        scope.Complete();
                        couldLock = true;
                    }
                    catch (FulcrumResourceLockedException)
                    {
                        couldLock = false;
                    }

                    tryLockHandler.Set();
                }
            });
            Assert.IsNotNull(thread1);
            thread1.Join();
            Assert.IsNotNull(thread2);
            thread2.Join();
            Assert.IsNotNull(couldLock);
            Assert.IsTrue(couldLock.Value);
        }

        /// <summary>
        /// Create a lock and then lock it again (which should fail).
        /// </summary>
        [TestMethod]
        public async Task ClaimTransactionLock_Given_AlreadyLocked_Gives_FulcrumResourceLockedException()
        {
            TId id = default;
            using (var scope = CreateNormalScope())
            {
                id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
                scope.Complete();
            }

            TestItemId<TId> lockedItem1 = null;
            bool? couldLock = null;
            var firstScopeDone = new ManualResetEvent(false);

            var thread = ThreadHelper.FireAndForget(async () =>
            {
                firstScopeDone.WaitOne(TimeSpan.FromSeconds(1));
                using (var scope2 = CreateNormalScope())
                {
                    try
                    {
                        var lockedItem2 = await CrdStorage.ClaimTransactionLockAndReadAsync(id);
                        Assert.IsNotNull(lockedItem2);
                        scope2.Complete();
                        couldLock = true;
                    }
                    catch (FulcrumResourceLockedException)
                    {
                        couldLock = false;
                    }
                }
            });
            using (var scope1 = CreateNormalScope())
            {
                try
                {
                    lockedItem1 = await CrdStorage.ClaimTransactionLockAndReadAsync(id);
                    Assert.IsNotNull(lockedItem1);
                    while (!couldLock.HasValue) await Task.Delay(10);
                    scope1.Complete();
                }
                finally
                {
                    firstScopeDone.Set();
                }
            }
            Assert.IsNotNull(thread);
            thread.Join();
            Assert.IsNotNull(couldLock);
            Assert.IsFalse(couldLock.Value);
        }
        /// <summary>
        /// Create a scope with our standard options
        /// </summary>
        /// <returns></returns>
        private static TransactionScope CreateNormalScope()
        {

            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted
            };
            return new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
        }
#endif
        #endregion
    }
}

