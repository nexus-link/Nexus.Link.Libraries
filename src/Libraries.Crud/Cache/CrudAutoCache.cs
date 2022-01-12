using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
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
        private CrudConvenience<TModelCreate, TModel, TId> _convenience;

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
            _convenience = new CrudConvenience<TModelCreate, TModel,TId>(this);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var createdItem = await _service.CreateAndReturnAsync(item, cancellationToken );
            await CacheSetAsync(createdItem, cancellationToken );
            return createdItem;
        }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var id = await _service.CreateAsync(item, cancellationToken );
            await CacheMaybeSetAsync(id, _service, cancellationToken );
            return id;
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var createdItem = await _service.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken );
            await CacheSetByIdAsync(id, createdItem, cancellationToken );
            return createdItem;
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            await _service.CreateWithSpecifiedIdAsync(id, item, cancellationToken );
            await CacheMaybeSetAsync(id, _service, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var item = await CacheGetByIdAsync(id, cancellationToken );
            if (item != null) return item;
            item = await _service.ReadAsync(id, cancellationToken );
            if (Equals(item, default(TModel))) return default;
            await CacheSetByIdAsync(id, item, cancellationToken );
            return item;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            if (limit == 0) limit = int.MaxValue;
            var itemsArray = await CacheGetAsync(limit, ReadAllCacheKey, cancellationToken );
            if (itemsArray != null) return itemsArray;
            var itemsCollection = await _service.ReadAllAsync(limit, cancellationToken );
            itemsArray = itemsCollection as TModel[] ?? itemsCollection.ToArray();
            CacheItemsInBackground(itemsArray, limit, ReadAllCacheKey);
            return itemsArray;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            if (limit == null) limit = PageInfo.DefaultLimit;
            var result = await CacheGetAsync(offset, limit.Value, ReadAllCacheKey, cancellationToken );
            if (result != null) return result;
            result = await _service.ReadAllWithPagingAsync(offset, limit.Value, cancellationToken );
            if (result?.Data == null) return null;
            CacheItemsInBackground(result, limit.Value, ReadAllCacheKey);
            return result;
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            return _service.SearchAsync(details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            await _service.UpdateAsync(id, item, cancellationToken );
            await CacheMaybeSetAsync(id, _service, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            var updatedItem = await _service.UpdateAndReturnAsync(id, item, cancellationToken );
            await CacheSetByIdAsync(id, updatedItem, cancellationToken );
            return updatedItem;

        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken cancellationToken  = default)
        {
            var task1 = FlushAsync(cancellationToken );
            var task2 = _service.DeleteAllAsync(cancellationToken );
            await Task.WhenAll(task1, task2);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TId id, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var task1 = CacheRemoveByIdAsync(id, cancellationToken );
            var task2 = _service.DeleteAsync(id, cancellationToken );
            await Task.WhenAll(task1, task2);
        }

        /// <inheritdoc />
        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _service.ClaimLockAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public Task ReleaseLockAsync(TId id, TId lockId, CancellationToken cancellationToken  = default)
        {
            return _service.ReleaseLockAsync(id, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, TimeSpan? lockTimeSpan = null, TId currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            return _service.ClaimDistributedLockAsync(id, lockTimeSpan, currentLockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken cancellationToken  = default)
        {
            return _service.ReleaseDistributedLockAsync(id, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _service.ClaimTransactionLockAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _convenience.ClaimTransactionLockAndReadAsync(id, cancellationToken );
        }
    }
}
