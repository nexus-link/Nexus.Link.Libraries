using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <inheritdoc cref="CrudAutoCache{TModelCreate, TModel,TId}" />
    public class CrudAutoCache<TModel, TId> : 
        CrudAutoCache<TModel, TModel, TId>,
        ICrud<TModel, TId>
    {
        /// <summary>
        /// Constructor for TModel that implements <see cref="IUniquelyIdentifiable{TId}"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        /// <param name="flushCacheDelegateAsync"></param>
        /// <param name="options"></param>
        public CrudAutoCache(ICrudable<TModel, TId> service, IDistributedCache cache,
            FlushCacheDelegateAsync flushCacheDelegateAsync = null, AutoCacheOptions options = null)
            : this(service, item => ((IUniquelyIdentifiable<TId>)item).Id, cache, flushCacheDelegateAsync, options)
        {
        }


        /// <summary>
        /// Constructor for TModel that does not implement <see cref="IUniquelyIdentifiable{TId}"/>, or when you want to specify your own GetKey() method.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        /// <param name="getIdDelegate"></param>
        /// <param name="flushCacheDelegateAsync"></param>
        /// <param name="options"></param>
        public CrudAutoCache(ICrudable<TModel, TId> service, GetIdDelegate<TModel, TId> getIdDelegate,
            IDistributedCache cache, FlushCacheDelegateAsync flushCacheDelegateAsync = null,
            AutoCacheOptions options = null)
            : base(service, getIdDelegate, cache, flushCacheDelegateAsync, options)
        {
        }
    }

    /// <inheritdoc cref="AutoCacheBase{TModel,TId}" />
    public class CrudAutoCache<TModelCreate, TModel, TId> : 
        AutoCacheBase<TModel, TId>, 
        ICrud<TModelCreate, TModel, TId> 
        where TModel : TModelCreate
    {
        private readonly ICrud<TModelCreate, TModel, TId> _service;

        /// <summary>
        /// Constructor for TModel that implements <see cref="IUniquelyIdentifiable{TId}"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        /// <param name="flushCacheDelegateAsync"></param>
        /// <param name="options"></param>
        public CrudAutoCache(ICrudable<TModel, TId> service, IDistributedCache cache, FlushCacheDelegateAsync flushCacheDelegateAsync = null, AutoCacheOptions options = null)
        : this(service, item => ((IUniquelyIdentifiable<TId>)item).Id, cache, flushCacheDelegateAsync, options)
        {
        }


        /// <summary>
        /// Constructor for TModel that does not implement <see cref="IUniquelyIdentifiable{TId}"/>, or when you want to specify your own GetKey() method.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        /// <param name="getIdDelegate"></param>
        /// <param name="flushCacheDelegateAsync"></param>
        /// <param name="options"></param>
        public CrudAutoCache(ICrudable<TModel, TId> service, GetIdDelegate<TModel, TId> getIdDelegate, IDistributedCache cache, FlushCacheDelegateAsync flushCacheDelegateAsync = null, AutoCacheOptions options = null)
            : base(getIdDelegate, cache, flushCacheDelegateAsync, options)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNull(getIdDelegate, nameof(getIdDelegate));
            InternalContract.RequireNotNull(cache, nameof(cache));
            _service = new CrudPassThrough<TModelCreate, TModel, TId>(service);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var createdItem = await _service.CreateAndReturnAsync(item, token);
            await CacheSetAsync(createdItem, token);
            return createdItem;
        }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var id = await _service.CreateAsync(item, token);
            await CacheMaybeSetAsync(id, _service, token);
            return id;
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var createdItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, item, token);
            await CacheSetByIdAsync(id, createdItem, token);
            return createdItem;
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            await _service.CreateWithSpecifiedIdAsync(id, item, token);
            await CacheMaybeSetAsync(id, _service, token);
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var item = await CacheGetByIdAsync(id, token);
            if (item != null) return item;
            item = await _service.ReadAsync(id, token);
            if (Equals(item, default(TModel))) return default(TModel);
            await CacheSetByIdAsync(id, item, token);
            return item;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            if (limit == 0) limit = int.MaxValue;
            var itemsArray = await CacheGetAsync(limit, ReadAllCacheKey, token);
            if (itemsArray != null) return itemsArray;
            var itemsCollection = await _service.ReadAllAsync(limit, token);
            itemsArray = itemsCollection as TModel[] ?? itemsCollection.ToArray();
            CacheItemsInBackground(itemsArray, limit, ReadAllCacheKey);
            return itemsArray;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            if (limit == null) limit = PageInfo.DefaultLimit;
            var result = await CacheGetAsync(offset, limit.Value, ReadAllCacheKey, token);
            if (result != null) return result;
            result = await _service.ReadAllWithPagingAsync(offset, limit.Value, token);
            if (result?.Data == null) return null;
            CacheItemsInBackground(result, limit.Value, ReadAllCacheKey);
            return result;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            await _service.UpdateAsync(id, item, token);
            await CacheMaybeSetAsync(id, _service, token);
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var updatedItem = await _service.UpdateAndReturnAsync(id, item, token);
            await CacheSetByIdAsync(id, updatedItem, token);
            return updatedItem;

        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            var task1 = FlushAsync(token);
            var task2 = _service.DeleteAllAsync(token);
            await Task.WhenAll(task1, task2);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var task1 = CacheRemoveByIdAsync(id, token);
            var task2 = _service.DeleteAsync(id, token);
            await Task.WhenAll(task1, task2);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            return _service.ClaimLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId id, TId lockId, CancellationToken token = default(CancellationToken))
        {
            return _service.ReleaseLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            return _service.ClaimDistributedLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken token = default(CancellationToken))
        {
            return _service.ReleaseDistributedLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            return _service.ClaimTransactionLockAsync(id, token);
        }
    }
}
