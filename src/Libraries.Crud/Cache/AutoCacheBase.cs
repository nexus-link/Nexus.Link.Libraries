using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Microsoft.Extensions.Caching.Distributed;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <summary>
    /// Use this to put an "intelligent" cache between you and your ICrud storage.
    /// </summary>
    public class AutoCacheBase<TModel, TId> : IFlushableCache
    {
        private int _limitOfItemsInReadAllCache;
        private readonly FlushCacheDelegateAsync _flushCacheDelegateAsync;
        private readonly ConcurrentDictionary<string, PageEnvelope<TModel>> _activeCachingOfPages = new ConcurrentDictionary<string, PageEnvelope<TModel>>();
        private readonly ConcurrentDictionary<string, bool> _collectionOperations = new ConcurrentDictionary<string, bool>();
        /// <summary>
        /// The cache key to use for the list of all items.
        /// </summary>
        protected const string ReadAllCacheKey = "ReadAllCacheKey";



        /// <summary>
        /// The cache of items
        /// </summary>
        protected readonly IDistributedCache Cache;

        /// <summary>
        /// A method for getting the id of an item.
        /// </summary>
        protected GetIdDelegate<TModel, TId> GetIdDelegate { get; }

        /// <summary>
        /// The options for the AutoCache
        /// </summary>
        protected AutoCacheOptions Options { get; }

        /// <summary>
        /// The options we should use when creating new items in the Distributed cache.
        /// </summary>
        protected  DistributedCacheEntryOptions CacheOptions { get; }

        /// <summary>
        /// The identity of the current cache. For caches that doesn't support flushing, this is how we forget about all old values.
        /// </summary>
        protected string CacheIdentity { get; set; }


        /// <summary>
        /// If you want to have your own method for discarding cache values, set this property to a method that returns how you want to deal with the cached value.
        /// </summary>
        public UseCacheStrategyDelegateAsync<TId> UseCacheStrategyMethodAsync { get; set; }

        /// <summary>
        /// If you want to be able to sometimes totally ignore the cache (saving yourself from a call to the cache), use this method to decide what to do.
        /// </summary>
        public UseCacheAtAllDelegateAsync UseCacheAtAllMethodAsync { get; set; }

        /// <summary>
        /// True while a background thread is active saving results from a ReadAll() operation.
        /// </summary>
        protected bool IsCollectionOperationActive(string key)
        {
            return _collectionOperations.ContainsKey(key);
        }

        /// <summary>
        /// True while a background thread is active saving results from a ReadAll() operation.
        /// </summary>
        protected bool IsCollectionOperationActive()
        {
            return IsCollectionOperationActive(ReadAllCacheKey);
        }

        /// <summary>
        /// Wait until any background thread is active saving results from a ReadAll() operation.
        /// </summary>
        public async Task DelayUntilNoOperationActiveAsync(string key = null, CancellationToken cancellationToken = default)
        {
            if (key == null) key = ReadAllCacheKey;
            var count = 0;
            while (count++ < 5 && !IsCollectionOperationActive(key)) await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);
            while (IsCollectionOperationActive(key)) await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
        }

        /// <summary>
        /// Constructor for TModel that implements <see cref="IUniquelyIdentifiable{TId}"/>.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="flushCacheDelegateAsync"></param>
        /// <param name="options"></param>
        public AutoCacheBase(IDistributedCache cache, FlushCacheDelegateAsync flushCacheDelegateAsync = null, AutoCacheOptions options = null)
        : this(item => item.GetPrimaryKey<TModel,TId>(), cache, flushCacheDelegateAsync, options)
        {
        }


        /// <summary>
        /// Constructor for TModel that does not implement <see cref="IUniquelyIdentifiable{TId}"/>, or when you want to specify your own GetKey() method.
        /// </summary>
        /// <param name="getIdDelegate"></param>
        /// <param name="cache"></param>
        /// <param name="flushCacheDelegateAsync"></param>
        /// <param name="options"></param>
        public AutoCacheBase(GetIdDelegate<TModel, TId> getIdDelegate, IDistributedCache cache, FlushCacheDelegateAsync flushCacheDelegateAsync = null, AutoCacheOptions options = null)
        {
            InternalContract.RequireNotNull(getIdDelegate, nameof(getIdDelegate));
            InternalContract.RequireNotNull(cache, nameof(cache));
            if (options == null)
            {
                options = new AutoCacheOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
            }

            CacheIdentity = Guid.NewGuid().ToString();
            Cache = cache;
            Options = options;
            GetIdDelegate = getIdDelegate;
            CacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = Options.SlidingExpiration
            };

            if (flushCacheDelegateAsync == null)
            {
                if (cache is IFlushableCache flushableCache)
                {
                    flushCacheDelegateAsync = flushableCache.FlushAsync;
                }
            }
            _flushCacheDelegateAsync = flushCacheDelegateAsync;
        }

        /// <summary>
        /// Add the items to the cache as a background process.
        /// </summary>
        protected internal void CacheItemsInBackground(TModel[] itemsArray, int limit, string key)
        {
            if (!_collectionOperations.TryAdd(key, true)) return;
            ThreadHelper.FireAndForget(async () =>
                await CacheItemCollectionOperationAsync(itemsArray, limit, key, true, CancellationToken.None).ConfigureAwait(false));
        }

        /// <summary>
        /// Add the items to the cache as a background process.
        /// </summary>
        protected internal void CacheItemsInBackground(PageEnvelope<TModel> pageEnvelope, int limit, string keyPrefix)
        {
            // Give up if this is an individual page being saved while we are operating on a larger scale, 
            // because that could lead to inconsistencies in the data
            if (IsCollectionOperationActive(keyPrefix)) return;

            var key = GetCacheKeyForPage(keyPrefix, pageEnvelope.PageInfo.Offset, pageEnvelope.PageInfo.Limit);
            // Give up if a save of the same page is already active
            if (!_activeCachingOfPages.TryAdd(key, pageEnvelope)) return;

            ThreadHelper.FireAndForget(async () =>
                await CacheItemPageOperationAsync(pageEnvelope, limit, keyPrefix, true, CancellationToken.None).ConfigureAwait(false));
        }

        /// <summary>
        /// Remove items from the cache as a background process.
        /// </summary>
        /// <param name="key">The key into the cache.</param>
        /// <param name="getItemsToDelete">A method that gets the items that should be deleted.</param>
        /// <param name="cancellationToken"></param>
        protected async Task RemoveCacheItemsInBackgroundAsync(string key, Func<Task<TModel[]>> getItemsToDelete, CancellationToken cancellationToken = default)
        {
            if (!_collectionOperations.TryAdd(key, true)) return;
            ThreadHelper.FireAndForget(() => ReadAndDeleteAsync(key, getItemsToDelete, cancellationToken));
            await Task.CompletedTask;
        }

        private async Task ReadAndDeleteAsync(string key, Func<Task<TModel[]>> getItemsToDelete, CancellationToken cancellationToken = default)
        {
            var itemsArray = await getItemsToDelete();
            if (itemsArray == null || itemsArray.Length == 0) return;
            await CacheItemCollectionOperationAsync(itemsArray, int.MaxValue, key, false, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove items from the cache as a background process.
        /// </summary>
        /// <param name="key">The key into the cache.</param>
        /// <param name="itemsArray">An array of items that should be removed from the cache.</param>
        protected void RemoveCacheItemsInBackground(TModel[] itemsArray, string key)
        {
            if (!_collectionOperations.TryAdd(key, true)) return;
            ThreadHelper.FireAndForget(async () =>
                await CacheItemCollectionOperationAsync(itemsArray, int.MaxValue, key, false, CancellationToken.None).ConfigureAwait(false));
        }

        /// <summary>
        /// Remove items from the cache as a background process.
        /// </summary>
        /// <param name="pageEnvelope">A page with the items to delete.</param>
        /// <param name="keyPrefix">The key into the cache.</param>
        /// 
        protected void RemoveCacheItemsInBackground(PageEnvelope<TModel> pageEnvelope, string keyPrefix)
        {
            // Give up if this is an individual page being saved while we are operating on a larger scale, 
            // because that could lead to inconsistencies in the data
            if (IsCollectionOperationActive(keyPrefix)) return;

            var key = GetCacheKeyForPage(keyPrefix, pageEnvelope.PageInfo.Offset, pageEnvelope.PageInfo.Limit);
            // Give up if a save of the same page is already active
            if (!_activeCachingOfPages.TryAdd(key, pageEnvelope)) return;

            ThreadHelper.FireAndForget(async () =>
                await CacheItemPageOperationAsync(pageEnvelope, int.MaxValue, keyPrefix, false, CancellationToken.None).ConfigureAwait(false));
        }

        private async Task CacheItemCollectionOperationAsync(TModel[] itemsArray, int limit, string key, bool isSetOperation, CancellationToken cancellationToken )
        {
            InternalContract.RequireNotNull(itemsArray, nameof(itemsArray));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            if (!isSetOperation)
            {
                InternalContract.Require(limit == int.MaxValue, $"When {nameof(isSetOperation)} is false, then {nameof(limit)} must be set to int.MaxValue ({int.MaxValue}), but it was set to {limit}.");
            }
            try
            {
                if (Options.SaveCollections)
                {

                    var cacheArrayTask = isSetOperation 
                        ? CacheSetAsync(itemsArray, limit, key, cancellationToken )
                        : Cache.RemoveAsync(key, cancellationToken );
                    var cachePageTasks = new List<Task>();

                    // Cache individual pages
                    var offset = 0;
                    while (offset < itemsArray.Length)
                    {
                        var data = itemsArray.Skip(offset).Take(PageInfo.DefaultLimit);
                        var pageEnvelope = new PageEnvelope<TModel>(offset, PageInfo.DefaultLimit, itemsArray.Length, data);
                        var task = CacheItemPageOperationAsync(pageEnvelope, limit, key, isSetOperation, cancellationToken );
                        cachePageTasks.Add(task);
                        offset += PageInfo.DefaultLimit;
                    }
                    await Task.WhenAll(cachePageTasks);
                    await cacheArrayTask;
                }
                else
                {
                    // Cache individual items
                    var cacheIndividualItemTasks = isSetOperation 
                    ? itemsArray.Select(item => CacheSetAsync(item, cancellationToken ))
                        : itemsArray.Select(item => CacheRemoveByIdAsync(GetIdDelegate(item), cancellationToken ));
                    await Task.WhenAll(cacheIndividualItemTasks);
                }
            }
            finally
            {
                _collectionOperations.TryRemove(key, out _);
            }
        }

        private async Task CacheItemPageOperationAsync(PageEnvelope<TModel> pageEnvelope, int limit, string keyPrefix, bool isSetOperation, CancellationToken cancellationToken )
        {
            var key = GetCacheKeyForPage(keyPrefix, pageEnvelope.PageInfo.Offset, pageEnvelope.PageInfo.Limit);
            try
            {
                var cacheIndividualItemTasks = isSetOperation
                    ? pageEnvelope.Data.Select(item => CacheSetAsync(item, cancellationToken ))
                    : pageEnvelope.Data.Select(item => CacheRemoveByIdAsync(GetIdDelegate(item), cancellationToken ));
                if (!PageWasTruncated(pageEnvelope.PageInfo, limit))
                {
                    if (isSetOperation) await CacheSetAsync(pageEnvelope, keyPrefix, cancellationToken );
                    else await Cache.RemoveAsync(key, cancellationToken );
                }
                await Task.WhenAll(cacheIndividualItemTasks);
            }
            finally
            {
                _activeCachingOfPages.TryRemove(key, out PageEnvelope<TModel> _);
            }
        }

        private bool PageWasTruncated(PageInfo pageInfo, int limit)
        {
            if (pageInfo.Total != null && limit > pageInfo.Total.Value) return false;
            var count = pageInfo.Offset * pageInfo.Limit + pageInfo.Returned;
            return count == limit;
        }

        /// <summary>
        /// Get a value from the cache if all constraints are fulfilled.
        /// </summary>
        protected internal async Task<PageEnvelope<TModel>> CacheGetAsync(int offset, int limit, string keyPrefix, CancellationToken cancellationToken )
        {
            if (UseCacheAtAllMethodAsync != null &&
                !await UseCacheAtAllMethodAsync(typeof(PageEnvelope<TModel>), cancellationToken )) return null;
            var key = GetCacheKeyForPage(keyPrefix, offset, limit);
            return await GetAndMaybeReturnAsync(key, cacheEnvelope => SerializingSupport.Deserialize<PageEnvelope<TModel>>(cacheEnvelope.Data), cancellationToken : cancellationToken );
        }

        /// <summary>
        /// Get a value from the cache if all constraints are fulfilled.
        /// </summary>
        protected internal async Task<TModel[]> CacheGetAsync(int limit, string key, CancellationToken cancellationToken )
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            if (limit > _limitOfItemsInReadAllCache) return null;
            if (UseCacheAtAllMethodAsync != null &&
                !await UseCacheAtAllMethodAsync(typeof(TModel[]), cancellationToken )) return null;

            return await GetAndMaybeReturnAsync(key, cacheEnvelope =>
                {
                    var array = SerializingSupport.Deserialize<TModel[]>(cacheEnvelope.Data);
                    if (limit > array.Length) return array;
                    var subset = array.Take(limit);
                    return subset as TModel[] ?? subset.ToArray();
                }, cancellationToken : cancellationToken );
        }

        /// <summary>
        /// Get a value from the cache if all constraints are fulfilled.
        /// </summary>
        private async Task<TModel> CacheGetAsync(TId id, string key, CancellationToken cancellationToken  = default)
        {
            if (UseCacheAtAllMethodAsync != null && !await UseCacheAtAllMethodAsync(typeof(TModel), cancellationToken )) return default;
            key = key ?? GetCacheKeyFromId(id);

            return await GetAndMaybeReturnAsync(key, cacheEnvelope => SerializingSupport.Deserialize<TModel>(cacheEnvelope.Data), id, cancellationToken );
        }

        /// <summary>
        /// Get a value from the cache if all constraints are fulfilled.
        /// </summary>
        protected async Task<TModel> CacheGetByIdAsync(TId id, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return await CacheGetAsync(id, null, cancellationToken );
        }

        /// <summary>
        /// Get a value from the cache if all constraints are fulfilled.
        /// </summary>
        protected async Task<TModel> CacheGetByKeyAsync(string key, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            return await CacheGetAsync(default(TId), key, cancellationToken );
        }

        private delegate TReturn DeserializeDelegate<out TReturn>(CacheEnvelope cacheEnvelope);

        private async Task<TReturn> GetAndMaybeReturnAsync<TReturn>(string key, DeserializeDelegate<TReturn> deserializeDelegate, TId id = default, CancellationToken cancellationToken  = default)
        {
            var byteArray = await Cache.GetAsync(key, cancellationToken );
            if (byteArray == null) return default;
            var cacheEnvelope = SerializingSupport.Deserialize<CacheEnvelope>(byteArray);
            var cacheItemStrategy = Equals(id, default(TId)) ? GetCacheItemStrategy(cacheEnvelope) : await GetCacheItemStrategyAsync(id, cacheEnvelope, cancellationToken );
            switch (cacheItemStrategy)
            {
                case UseCacheStrategyEnum.Use:
                    return deserializeDelegate(cacheEnvelope);
                case UseCacheStrategyEnum.Ignore:
                    return default;
                case UseCacheStrategyEnum.Remove:
                    await Cache.RemoveAsync(key, cancellationToken );
                    return default;
                default:
                    FulcrumAssert.Fail($"Unexpected value of {nameof(UseCacheStrategyEnum)} ({cacheItemStrategy}).");
                    return default;
            }
        }

        private async Task<UseCacheStrategyEnum> GetCacheItemStrategyAsync(TId id, CacheEnvelope cacheEnvelope, CancellationToken cancellationToken )
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(cacheEnvelope, nameof(cacheEnvelope));

            var cacheItemStrategy = GetCacheItemStrategy(cacheEnvelope);
            if (cacheItemStrategy != UseCacheStrategyEnum.Use) return cacheItemStrategy;
            if (UseCacheStrategyMethodAsync == null) return UseCacheStrategyEnum.Use;
            var cachedItemInformation = new CachedItemInformation<TId>
            {
                Id = id,
                UpdatedAt = cacheEnvelope.UpdatedAt
            };
            return await UseCacheStrategyMethodAsync(cachedItemInformation, cancellationToken );
        }

        private UseCacheStrategyEnum GetCacheItemStrategy(CacheEnvelope cacheEnvelope)
        {
            InternalContract.RequireNotNull(cacheEnvelope, nameof(cacheEnvelope));

            if (cacheEnvelope.CacheIdentity != CacheIdentity) return UseCacheStrategyEnum.Remove;
            return TooOld(cacheEnvelope) ? UseCacheStrategyEnum.Remove : UseCacheStrategyEnum.Use;
        }

        private bool TooOld(CacheEnvelope cacheEnvelope)
        {
            if (Options.AbsoluteExpirationRelativeToNow == null) return false;
            return cacheEnvelope.UpdatedAt.Add(Options.AbsoluteExpirationRelativeToNow.Value) <= DateTimeOffset.Now;
        }

        /// <summary>
        /// Put the item in the cache.
        /// </summary>
        protected Task CacheSetAsync(TModel item, CancellationToken cancellationToken )
        {
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            return CacheSetAsync(GetIdDelegate(item), item, null, cancellationToken );
        }

        /// <summary>
        /// Put the item in the cache.
        /// </summary>
        protected async Task CacheSetByIdAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            await CacheSetAsync(id, item, null, cancellationToken );
        }

        /// <summary>
        /// Put the item in the cache.
        /// </summary>
        protected async Task CacheSetByKeyAsync(string key, TModel item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            await CacheSetAsync(default, item, key, cancellationToken );
        }

        /// <summary>
        /// Put the item in the cache.
        /// </summary>
        private async Task CacheSetAsync(TId id, TModel item, string key = null, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(item, nameof(item));
            key = key ?? GetCacheKeyFromId(id);
            var serializedCacheEnvelope = ToSerializedCacheEnvelope(item);
            await Cache.SetAsync(key, serializedCacheEnvelope, CacheOptions, cancellationToken );
        }

        /// <summary>
        /// Put the item in the cache.
        /// </summary>
        private async Task CacheSetAsync(TModel[] itemsArray, int limit, string key, CancellationToken cancellationToken )
        {
            InternalContract.RequireNotDefaultValue(itemsArray, nameof(itemsArray));
            var serializedCacheEnvelope = ToSerializedCacheEnvelope(itemsArray);
            await Cache.SetAsync(key, serializedCacheEnvelope, CacheOptions, cancellationToken );
            _limitOfItemsInReadAllCache = limit;
        }

        /// <summary>
        /// Put the item in the cache.
        /// </summary>
        private async Task CacheSetAsync(PageEnvelope<TModel> pageEnvelope, string keyPrefix, CancellationToken cancellationToken )
        {
            InternalContract.RequireNotDefaultValue(pageEnvelope, nameof(pageEnvelope));
            InternalContract.RequireValidated(pageEnvelope, nameof(pageEnvelope));
            var serializedCacheEnvelope = ToSerializedCacheEnvelope(pageEnvelope);
            var key = GetCacheKeyForPage(keyPrefix, pageEnvelope.PageInfo.Offset, pageEnvelope.PageInfo.Limit);
            await Cache.SetAsync(key, serializedCacheEnvelope, CacheOptions, cancellationToken );
        }

        /// <summary>
        /// Read and cache the item, but only if we have an aggressive caching strategy.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <param name="service">A service to use for reading the item.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        protected async Task CacheMaybeSetAsync(TId id, IRead<TModel, TId> service, CancellationToken cancellationToken  = default)
        {
            async Task<bool> IsAlreadyCachedAndGetIsOkToUpdate()
            {
                return Options.DoGetToUpdate && await CacheItemExistsAsync(id, cancellationToken );
            }

            var getAndSave = Options.SaveAll || await IsAlreadyCachedAndGetIsOkToUpdate();
            if (!getAndSave) return;
            var item = await service.ReadAsync(id, cancellationToken );
            await CacheSetByIdAsync(id, item, cancellationToken );
        }

        /// <summary>
        /// Check if an item exists in the cache.
        /// </summary>
        /// <param name="id">The id for the item</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        protected async Task<bool> CacheItemExistsAsync(TId id, CancellationToken cancellationToken )
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var key = GetCacheKeyFromId(id);
            var cachedItem = await Cache.GetAsync(key, cancellationToken );
            return cachedItem != null;
        }

        /// <summary>
        /// Get the cache key for a specific page
        /// </summary>
        protected static string GetCacheKeyForPage(string prefix, int offset, int limit)
        {
            return $"{prefix}-{offset}-{limit}";
        }

        /// <summary>
        /// Get the cacke key
        /// </summary>
        protected static string GetCacheKeyFromId(TId id)
        {
            var key = id?.ToString();
            InternalContract.Require(key != null,
                $"Could not extract a cache key for an item of type {typeof(TModel).FullName}.");
            return key;
        }

        /// <summary>
        /// Remove from cache
        /// </summary>
        protected async Task CacheRemoveByIdAsync(TId id, CancellationToken cancellationToken )
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            var key = GetCacheKeyFromId(id);
            await Cache.RemoveAsync(key, cancellationToken );
        }

        /// <summary>
        /// Serialize the <paramref name="item"/>, put it into an envelope and serialize the envelope
        /// </summary>
        public byte[] ToSerializedCacheEnvelope<T>(T item)
        {
            var serializedItem = SerializingSupport.Serialize(item);
            var cacheEnvelope = new CacheEnvelope
            {
                CacheIdentity = CacheIdentity,
                UpdatedAt = DateTimeOffset.Now,
                Data = serializedItem
            };
            var serializedCacheEnvelope = SerializingSupport.Serialize(cacheEnvelope);
            return serializedCacheEnvelope;
        }

        /// <inheritdoc />
        public async Task FlushAsync(CancellationToken cancellationToken  = default)
        {
            if (_flushCacheDelegateAsync == null)
            {
                var message = $"When deleting all items from the storage, a flush method for the cache was not specified for the model {typeof(TModel).FullName}, so we can't flush the items from the cache." +
                              "The items added before this will be ignored and there is no risk for inconsistency, but the cache keeps growing.";
                Log.LogWarning(message);
                CacheIdentity = Guid.NewGuid().ToString();
                return;
            }

            await _flushCacheDelegateAsync(cancellationToken );
            CacheIdentity = Guid.NewGuid().ToString();
        }
    }
}
