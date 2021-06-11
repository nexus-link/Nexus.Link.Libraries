using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Cache;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;

namespace Nexus.Link.Libraries.Crud.Test.Cache
{
    [TestClass]
    public class TestAutoCacheCrud : TestAutoCacheBase<string, string>
    {
        private CrudAutoCache<string, Guid> _autoCache;

        /// <inheritdoc />
        public override CrudAutoCache<string, string, Guid> CrudAutoCache => _autoCache;

        private ICrud<string, string, Guid> _storage;
        /// <inheritdoc />
        protected override ICrud<string, string, Guid> CrudStorage => _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestAutoCacheCrud).FullName);
            _storage = new CrudMemory<string, string, Guid>();
            Cache = new CrudMemoryDistributedCache();
            DistributedCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1000)
            };
            AutoCacheOptions = new AutoCacheOptions
            {
                AbsoluteExpirationRelativeToNow = DistributedCacheOptions.AbsoluteExpirationRelativeToNow
            };
            _autoCache = new CrudAutoCache<string, Guid>(_storage, ToGuid, Cache, null, AutoCacheOptions);
        }

        [TestMethod]
        public async Task CreateStorageWithId_ReadCache()
        {
            var id = Guid.NewGuid();
            await _autoCache.CreateWithSpecifiedIdAndReturnAsync(id, "A");
            await PrepareStorageAsync(id, "B");
            await VerifyAsync(id, "B", "A");
        }

        /// <summary>
        /// The DoGetToUpdate setting does not matter when creating items.
        /// </summary>
        [TestMethod]
        public async Task CreateStorageWithId_ReadStorage()
        {
            var doGetToUpdate = false;
            while (true)
            {
                AutoCacheOptions.DoGetToUpdate = doGetToUpdate;
                var id = Guid.NewGuid();
                await _autoCache.CreateWithSpecifiedIdAsync(id, "A"); // Will not update cache
                await PrepareStorageAsync(id, "B");
                await VerifyAsync(id, "B", null, "B");
                if (doGetToUpdate) break;
                doGetToUpdate = true;
            }
        }

        [TestMethod]
        public async Task CreateStorageWithIdWith_SaveOption_ReadCache()
        {
            AutoCacheOptions.SaveAll = true;
            _autoCache = new CrudAutoCache<string, Guid>(_storage, ToGuid, Cache, null, AutoCacheOptions);
            var id = Guid.NewGuid();
            await _autoCache.CreateWithSpecifiedIdAsync(id, "A"); // Will update cache thanks to SaveAll
            await PrepareStorageAsync(id, "B");
            await VerifyAsync(id, "B", "A");
        }

        [TestMethod]
        public async Task CreateStorage_ReadCache()
        {
            await _autoCache.CreateAndReturnAsync("A");
            var id = ToGuid("A");
            await PrepareStorageAsync(id, "B");
            await VerifyAsync(id, "B", "A");
        }

        /// <summary>
        /// The DoGetToUpdate setting does not matter when creating items.
        /// </summary>

        [TestMethod]
        public async Task CreateStorage_ReadStorage()
        {
            var doGetToUpdate = false;
            while (true)
            {
                AutoCacheOptions.DoGetToUpdate = doGetToUpdate;
                var id = await _autoCache.CreateAsync("A"); // Will not update cache
                await PrepareStorageAsync(id, "B");
                await VerifyAsync(id, "B", null, "B");
                if (doGetToUpdate) break;
                doGetToUpdate = true;
            }
        }

        [TestMethod]
        public async Task CreateStorage_SaveOption_ReadCache()
        {
            AutoCacheOptions.SaveAll = true;
            _autoCache = new CrudAutoCache<string, Guid>(_storage, ToGuid, Cache, null, AutoCacheOptions);
            var id = await _autoCache.CreateAsync("A"); // Will update cache thanks to SaveAll
            await PrepareStorageAsync(id, "B");
            await VerifyAsync(id, "B", "A");
        }

        [TestMethod]
        public async Task ReadStorage()
        {
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", null);
            await VerifyAsync(id, "A", null, "A");
        }

        [TestMethod]
        public async Task ReadStorage_ReadCache()
        {
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", null);
            await _autoCache.ReadAsync(id);
            await VerifyAsync(id, "A");
        }

        [TestMethod]
        public async Task ReadStorage_MethodPrevent_ReadStorage()
        {
            _autoCache.UseCacheAtAllMethodAsync = (type, ct)  => Task.FromResult(false);
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", "A");
            await PrepareStorageAsync(id, "B");
            await VerifyAsync(id, "B", "A", "B");
        }

        [TestMethod]
        public async Task ReadStorage_MethodIgnore_ReadStorage()
        {
            _autoCache.UseCacheStrategyMethodAsync = (type, t) => Task.FromResult(UseCacheStrategyEnum.Ignore);
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", null);
            await VerifyAsync(id, "A", null, "A");
            await PrepareStorageAsync(id, "B");
            await VerifyAsync(id, "B", "A", "B");
        }

        [TestMethod]
        public async Task ReadStorage_MethodRemove_ReadStorage()
        {
            _autoCache.UseCacheStrategyMethodAsync = (type, t) => Task.FromResult(UseCacheStrategyEnum.Remove);
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", null);
            await VerifyAsync(id, "A", null, "A");
            await PrepareStorageAsync(id, "B");
            await VerifyAsync(id, "B", "A", "B");
        }

        [TestMethod]
        public async Task ReadStorage_ReadCacheTooLongDelay_ReadStorage()
        {
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", null);
            await VerifyAsync(id, "A", null, "A");
            await PrepareStorageAsync(id, "B");
            Assert.IsNotNull(AutoCacheOptions.AbsoluteExpirationRelativeToNow);
            await Task.Delay(AutoCacheOptions.AbsoluteExpirationRelativeToNow.Value.Add(TimeSpan.FromMilliseconds(100)));
            await VerifyAsync(id, "B", "A", "B");

        }

        [TestMethod]
        public async Task ReadAll()
        {
            AutoCacheOptions.SaveCollections = true;
            AutoCacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            _autoCache = new CrudAutoCache<string, Guid>(_storage, item => ToGuid(item, 1), Cache, null, AutoCacheOptions);
            var id1 = ToGuid("A1", 1);
            await PrepareStorageAndCacheAsync(id1, "A1", null);
            var id2 = ToGuid("B1", 1);
            await PrepareStorageAndCacheAsync(id2, "B1", null);
            var result = await _autoCache.ReadAllAsync();
            Assert.IsNotNull(result);
            var enumerable = result as string[] ?? result.ToArray();
            Assert.AreEqual(2, enumerable.Length);
            Assert.IsTrue(enumerable.Contains("A1"));
            Assert.IsTrue(enumerable.Contains("B1"));
            await _autoCache.DelayUntilNoOperationActiveAsync();

            await _storage.UpdateAsync(id1, "A2");
            await _storage.UpdateAsync(id2, "B2");
            // Even though the items have been updated, the result will be fetched from the cache.
            result = await _autoCache.ReadAllAsync();
            Assert.IsNotNull(result);
            enumerable = result as string[] ?? result.ToArray();
            Assert.AreEqual(2, enumerable.Length);
            Assert.IsTrue(enumerable.Contains("A1"), "Missing A1 in " + string.Join(", ", enumerable));
            Assert.IsTrue(enumerable.Contains("B1"), "Missing B1 in " + string.Join(", ", enumerable));
        }

        [TestMethod]
        public async Task ReadAllUpdatesIndividualItems()
        {
            AutoCacheOptions.SaveCollections = true;
            AutoCacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            _autoCache = new CrudAutoCache<string, Guid>(_storage, item => ToGuid(item, 1), Cache, null, AutoCacheOptions);
            var id1 = ToGuid("A1", 1);
            await PrepareStorageAndCacheAsync(id1, "A1", null);
            var id2 = ToGuid("B1", 1);
            await PrepareStorageAndCacheAsync(id2, "B1", null);
            var result = await _autoCache.ReadAllAsync();
            Assert.IsNotNull(result);
            await _autoCache.DelayUntilNoOperationActiveAsync();
            await VerifyAsync(id1, "A1");
            await VerifyAsync(id2, "B1");
        }

        [TestMethod]
        public void TestFromStringToGuid()
        {
            // Not equal
            VerifyFromStringToGuidAreNotEqual("A", "B");
            VerifyFromStringToGuidAreNotEqual("A1", "A2");

            // Equal
            VerifyFromStringToGuidAreEqual("A1", "A2", 1);
        }

        [TestMethod]
        public async Task UpdateStorage_ReadNewCache()
        {
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", "A");
            await _autoCache.UpdateAndReturnAsync(id, "B");
            await PrepareStorageAsync(id, "C");
            await VerifyAsync(id, "C", "B");
        }

        [TestMethod]
        public async Task UpdateStorage_ReadOldCache()
        {
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", "A");
            await _autoCache.UpdateAsync(id, "B"); // Will not update cache
            await VerifyAsync(id, "B", "A");
        }

        [TestMethod]
        public async Task UpdateStorage_GetOption_ReadCache()
        {
            AutoCacheOptions.DoGetToUpdate = true;
            _autoCache = new CrudAutoCache<string, Guid>(_storage, ToGuid, Cache, null, AutoCacheOptions);
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", "A");
            await _autoCache.UpdateAsync(id, "B"); // Will update cache thanks to DoGetToUpdate
            await PrepareStorageAsync(id, "C");
            await VerifyAsync(id, "C", "B");
        }

        [TestMethod]
        public async Task UpdateStorage_SaveOption_ReadCache()
        {
            AutoCacheOptions.SaveAll = true;
            _autoCache = new CrudAutoCache<string, Guid>(_storage, ToGuid, Cache, null, AutoCacheOptions);
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", "A");
            await _autoCache.UpdateAsync(id, "B"); // Will update cache thanks to SaveAll
            await PrepareStorageAsync(id, "C");
            await VerifyAsync(id, "C", "B");
        }

        [TestMethod]
        public async Task DeleteRemovesCache()
        {
            var id = Guid.NewGuid();
            await PrepareStorageAndCacheAsync(id, "A", "A");
            await _autoCache.DeleteAsync(id);
            await VerifyAsync(id, null);
        }

        [TestMethod]
        public async Task DeleteAllRemovesCache()
        {
            AutoCacheOptions.SaveCollections = true;
            AutoCacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            _autoCache = new CrudAutoCache<string, Guid>(_storage, item => ToGuid(item, 1), Cache, null, AutoCacheOptions);
            var id1 = ToGuid("A1", 1);
            await PrepareStorageAndCacheAsync(id1, "A1", null);
            var id2 = ToGuid("B1", 1);
            await PrepareStorageAndCacheAsync(id2, "B1", null);
            await _autoCache.ReadAllAsync();
            await _autoCache.DeleteAllAsync();

            var result = await _autoCache.ReadAllAsync();
            Assert.IsNotNull(result);
            var enumerable = result as string[] ?? result.ToArray();
            Assert.AreEqual(0, enumerable.Length);
        }

        private void VerifyFromStringToGuidAreNotEqual(string a, string b, int maxLength = Int32.MaxValue)
        {
            var id1 = ToGuid(a, maxLength);
            var id2 = ToGuid(b, maxLength);
            Assert.AreNotEqual(id1, id2);
        }
        private void VerifyFromStringToGuidAreEqual(string a, string b, int maxLength = Int32.MaxValue)
        {
            var id1 = ToGuid(a, maxLength);
            var id2 = ToGuid(b, maxLength);
            Assert.AreEqual(id1, id2);
        }
    }
}
