using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Nexus.Link.Libraries.Crud.Cache;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.Test.Cache
{
    public abstract class TestAutoCacheBase<TModelCreate, TModel>
    where TModel : TModelCreate
    {
        protected IDistributedCache Cache;

        protected virtual ICrud<TModelCreate, TModel, Guid> CrudStorage { get; }
        protected DistributedCacheEntryOptions DistributedCacheOptions;
        protected readonly string BaseGuidString;
        protected AutoCacheOptions AutoCacheOptions;

        public virtual CrudAutoCache<TModelCreate, TModel, Guid> CrudAutoCache { get; }

        protected TestAutoCacheBase()
        {
            BaseGuidString = Guid.NewGuid().ToString();
        }

        protected async Task PrepareStorageAndCacheAsync(Guid id, TModel storageValue, TModel cacheValue, CancellationToken cancellationToken = default)
        {
            await PrepareStorageAsync(id, storageValue, cancellationToken);
            await PrepareCacheAsync(id, cacheValue, cancellationToken);
        }

        protected async Task PrepareCacheAsync(Guid id, TModel cacheValue, CancellationToken cancellationToken = default)
        {
            if (cacheValue == null)
            {
                await Cache.RemoveAsync(id.ToString(), cancellationToken);
            }
            else
            {
                await Cache.SetAsync(id.ToString(), CrudAutoCache.ToSerializedCacheEnvelope(cacheValue), DistributedCacheOptions, cancellationToken);
            }
        }

        protected async Task PrepareStorageAsync(Guid id, TModel storageValue, CancellationToken cancellationToken = default)
        {
            if (storageValue == null)
            {
                await CrudStorage.DeleteAsync(id, cancellationToken);
            }
            else
            {
                var value = await CrudStorage.ReadAsync(id, cancellationToken);
                if (value == null) await CrudStorage.CreateWithSpecifiedIdAsync(id, storageValue, cancellationToken);
                else if (!Equals(value, storageValue)) await CrudStorage.UpdateAsync(id, storageValue, cancellationToken);
            }
        }

        protected async Task VerifyAsync(Guid id, TModel expectedStorageValue, TModel expectedCacheValueBefore, TModel expectedReadValue, TModel expectedCacheValueAfter, CancellationToken cancellationToken = default)
        {
            await VerifyStorage(id, expectedStorageValue, cancellationToken);
            await VerifyCache(id, expectedCacheValueBefore, true, cancellationToken);
            await VerifyRead(id, expectedReadValue, cancellationToken);
            await VerifyCache(id, expectedCacheValueAfter, false, cancellationToken);
        }

        protected async Task VerifyAsync(Guid id, TModel expectedStorageValue, TModel expectedCacheValueBefore, TModel expectedReadValue, CancellationToken cancellationToken = default)
        {
            await VerifyStorage(id, expectedStorageValue, cancellationToken);
            await VerifyCache(id, expectedCacheValueBefore, true, cancellationToken);
            await VerifyRead(id, expectedReadValue, cancellationToken);
            await VerifyCache(id, expectedReadValue, false, cancellationToken);
        }

        protected async Task VerifyAsync(Guid id, TModel expectedStorageValue, TModel expectedCacheValueBefore, CancellationToken cancellationToken = default)
        {
            await VerifyStorage(id, expectedStorageValue, cancellationToken);
            await VerifyCache(id, expectedCacheValueBefore, true, cancellationToken);
            await VerifyRead(id, expectedCacheValueBefore, cancellationToken);
            await VerifyCache(id, expectedCacheValueBefore, false, cancellationToken);
        }

        protected async Task VerifyAsync(Guid id, TModel expectedStorageValue, CancellationToken cancellationToken = default)
        {
            await VerifyStorage(id, expectedStorageValue, cancellationToken);
            await VerifyCache(id, expectedStorageValue, true, cancellationToken);
            await VerifyRead(id, expectedStorageValue, cancellationToken);
            await VerifyCache(id, expectedStorageValue, false, cancellationToken);
        }

        protected async Task VerifyStorage(Guid id, TModel expectedStorageValue, CancellationToken cancellationToken = default)
        {
            var actualStorageValue = await CrudStorage.ReadAsync(id, cancellationToken);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedStorageValue, actualStorageValue, "Storage verification failed.");
        }

        protected async Task VerifyCache(Guid id, TModel expectedCacheValue, bool isBeforeRead, CancellationToken cancellationToken = default)
        {
            var beforeOrAfter = isBeforeRead ? "before" : "after";
            var actualCacheSerializedValue = await Cache.GetAsync(id.ToString(), cancellationToken);
            if (actualCacheSerializedValue == null)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(expectedCacheValue, $"Cache value was null, but expected \"{expectedCacheValue}\" {beforeOrAfter} read.");
            }
            else
            {
                var actualCacheValue = SerializingSupport.ToItem<TModel>(actualCacheSerializedValue);
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedCacheValue, actualCacheValue, $"Cache verification {beforeOrAfter} read failed.");
            }
        }

        protected async Task VerifyRead(Guid id, TModel expectedReadValue, CancellationToken cancellationToken = default)
        {
            var actualReadValue = await CrudAutoCache.ReadAsync(id, cancellationToken);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expectedReadValue, actualReadValue, "CrudAutoCache Read verification failed.");
        }

        protected Guid ToGuid(TModel item)
        {
            return ToGuid(item, int.MaxValue);
        }

        protected Guid ToGuid(TModel item, int maxLength)
        {
            var itemAsString = item.ToString();
            if (itemAsString.Length > maxLength)
            {
                itemAsString = itemAsString.Substring(0, maxLength);
            }
            var itemAsInt = itemAsString.GetHashCode();
            var itemAsHex = itemAsInt.ToString("X");
            var itemLength = itemAsHex.Length;
            var totalLength = BaseGuidString.Length;
            var itemAsGuidString = BaseGuidString.Substring(0, totalLength - itemLength) + itemAsHex;
            return new Guid(itemAsGuidString);
        }
    }
}
