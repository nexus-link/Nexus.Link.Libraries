﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Cache;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Crud.Test.Cache
{
    [TestClass]
    public class TestAutoCacheManyToOneRecursive : TestAutoCacheBase<ItemWithParentId, ItemWithParentId>
    {
        private ManyToOneAutoCache<ItemWithParentId, Guid> _autoCache;

        private ICrudManyToOne<ItemWithParentId, Guid> _storage;
        /// <inheritdoc />
        protected override ICrud<ItemWithParentId, ItemWithParentId, Guid> CrudStorage => _storage;
        
        /// <inheritdoc />
        public override CrudAutoCache<ItemWithParentId, ItemWithParentId, Guid> CrudAutoCache => _autoCache;


        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestAutoCacheManyToOneRecursive).FullName);
            _storage = new ManyToOneMemory<ItemWithParentId, Guid>(item => item.ParentId);
            Cache = new CrudMemoryDistributedCache();
            DistributedCacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(1000)
            };
            AutoCacheOptions = new AutoCacheOptions
            {
                AbsoluteExpirationRelativeToNow = DistributedCacheOptions.AbsoluteExpirationRelativeToNow
            };
            _autoCache = new ManyToOneAutoCache<ItemWithParentId, Guid>(_storage, Cache, null, AutoCacheOptions);
        }

        [TestMethod]
        public async Task ReadChildren()
        {
            AutoCacheOptions.SaveCollections = true;
            AutoCacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            _autoCache = new ManyToOneAutoCache<ItemWithParentId, Guid>(_storage, Cache, null, AutoCacheOptions);
            var parentId = Guid.NewGuid();
            var parent = new ItemWithParentId(parentId, "ParentA");
            await PrepareStorageAndCacheAsync(parentId, parent, null);
            var childId1 = Guid.NewGuid();
            var child1A = new ItemWithParentId(childId1, "Child1A", parentId);
            await PrepareStorageAndCacheAsync(childId1, child1A, null);
            var childId2 = Guid.NewGuid();
            var child2A = new ItemWithParentId(childId2, "Child2A", parentId);
            await PrepareStorageAndCacheAsync(childId2, child2A, null);
            var result = await _autoCache.ReadChildrenAsync(parentId);
            UT.Assert.IsNotNull(result);
            var enumerable = result as ItemWithParentId[] ?? result.ToArray();
            UT.Assert.AreEqual(2, enumerable.Length);
            UT.Assert.IsTrue(enumerable.Contains(child1A));
            UT.Assert.IsTrue(enumerable.Contains(child2A));
            await _autoCache.DelayUntilNoOperationActiveAsync();

            var child1B = new ItemWithParentId(childId1, "Child1B", parentId);
            await _storage.UpdateAsync(childId1, child1B);
            var child2B = new ItemWithParentId(childId2, "Child2B", parentId);
            await _storage.UpdateAsync(childId2, child2B);
            // Even though the items have been updated, the result will be fetched from the cache.
            result = await _autoCache.ReadChildrenAsync(parentId);
            UT.Assert.IsNotNull(result);
            enumerable = result as ItemWithParentId[] ?? result.ToArray();
            UT.Assert.AreEqual(2, enumerable.Length);
            UT.Assert.IsTrue(enumerable.Contains(child1A), $"Missing {child1A.Value} in " + string.Join(", ", enumerable.Select(item => item.Value)));
            UT.Assert.IsTrue(enumerable.Contains(child2A), $"Missing {child2A.Value} in " + string.Join(", ", enumerable.Select(item => item.Value)));
        }

        [TestMethod]
        public async Task ReadChildrenUpdatesIndividualItems()
        {
            AutoCacheOptions.SaveCollections = true;
            AutoCacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            _autoCache = new ManyToOneAutoCache<ItemWithParentId, Guid>(_storage, Cache, null, AutoCacheOptions);
            var parentId = Guid.NewGuid();
            var parent = new ItemWithParentId(parentId, "ParentA");
            await PrepareStorageAndCacheAsync(parentId, parent, null);
            var childId1 = Guid.NewGuid();
            var child1A = new ItemWithParentId(childId1, "Child1A", parentId);
            await PrepareStorageAndCacheAsync(childId1, child1A, null);
            var childId2 = Guid.NewGuid();
            var child2A = new ItemWithParentId(childId2, "Child2A", parentId);
            await PrepareStorageAndCacheAsync(childId2, child2A, null);
            var result = await _autoCache.ReadChildrenAsync(parentId);
            UT.Assert.IsNotNull(result);
            await _autoCache.DelayUntilNoOperationActiveAsync(parentId);
            await VerifyAsync(childId1, child1A);
            await VerifyAsync(childId2, child2A);
        }

        [TestMethod]
        public async Task DeleteChildren()
        {
            AutoCacheOptions.SaveCollections = true;
            AutoCacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            _autoCache = new ManyToOneAutoCache<ItemWithParentId, Guid>(_storage, Cache, null, AutoCacheOptions);
            var parentId = Guid.NewGuid();
            var parent = new ItemWithParentId(parentId, "ParentA");
            await PrepareStorageAndCacheAsync(parentId, parent, null);
            var childId1 = Guid.NewGuid();
            var child1A = new ItemWithParentId(childId1, "Child1A", parentId);
            await PrepareStorageAndCacheAsync(childId1, child1A, null);
            var childId2 = Guid.NewGuid();
            var child2A = new ItemWithParentId(childId2, "Child2A", parentId);
            await PrepareStorageAndCacheAsync(childId2, child2A, null);
            // Read into cache
            await _autoCache.ReadChildrenAsync(parentId);
            await _autoCache.DeleteChildrenAsync(parentId);
            await _autoCache.DelayUntilNoOperationActiveAsync(parentId);

            // Even though the items have been updated, the result will be fetched from the cache.
            var result = await _autoCache.ReadChildrenAsync(parentId);
            UT.Assert.IsNotNull(result);
            var enumerable = result as ItemWithParentId[] ?? result.ToArray();
            UT.Assert.AreEqual(0, enumerable.Length);
        }
    }
}
