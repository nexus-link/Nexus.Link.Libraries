﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{
    /// <summary>
    /// General class for storing any <see cref="IUniquelyIdentifiable{TId}"/> in memory.
    /// </summary>
    /// <typeparam name="TModel">The type of objects that are returned from persistent storage.</typeparam>
    /// <typeparam name="TId"></typeparam>
    public class CrudMemory<TModel, TId> :
        CrudMemory<TModel, TModel, TId>,
        ICrud<TModel, TId>
    {
    }

    /// <summary>
    /// General class for storing any <see cref="IUniquelyIdentifiable{TId}"/> in memory.
    /// </summary>
    /// <typeparam name="TModelCreate">The type for creating objects in persistent storage.</typeparam>
    /// <typeparam name="TModel">The type of objects that are returned from persistent storage.</typeparam>
    /// <typeparam name="TId"></typeparam>
    public class CrudMemory<TModelCreate, TModel, TId> :
        MemoryBase<TModel, TId>,
        ICrud<TModelCreate, TModel, TId> where TModel : TModelCreate
    {
        private readonly CrudConvenience<TModelCreate, TModel, TId> _convenience;
        private readonly object _lock = new object();
        private int _nextIntegerId = 1;

        /// <summary>
        /// Delegate for verifying that an item is unique before creating it.
        /// </summary>
        /// <param name="item">The item that should be verified for uniqueness.</param>
        /// <returns>A "where" object that can be used as the parameter for the <see cref="SearchDetails{TModel}"/> constructor.</returns>
        public delegate object UniqueConstraintDelegate(TModelCreate item);

        public UniqueConstraintDelegate UniqueConstraintMethods { get; set; }

        public CrudMemory()
        {
            _convenience = new CrudConvenience<TModelCreate, TModel, TId>(this);
        }

        /// <inheritdoc />
        public virtual async Task<TId> CreateAsync(TModelCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var id = CreateNewId<TId>();
            await CreateWithSpecifiedIdAsync(id, item, cancellationToken);
            return id;
        }

        protected internal T CreateNewId<T>()
        {
            T id;
            if (typeof(TId) == typeof(int))
            {
                lock (_lock)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    id = (dynamic)_nextIntegerId++;
                }
            }
            else
            {
                id = StorageHelper.CreateNewId<T>();
            }
            return id;
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var id = await CreateAsync(item, cancellationToken);
            return await ReadAsync(id, cancellationToken);
        }

        private readonly NexusAsyncSemaphore _semaphore = new NexusAsyncSemaphore();

        /// <inheritdoc />
        public virtual async Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var itemCopy = CopyItem(item);
            itemCopy.TrySetOptimisticConcurrencyControl();
            StorageHelper.MaybeUpdateTimeStamps(itemCopy, true);

            await _semaphore.ExecuteAsync((ct) => InternalCreateAsync(id, itemCopy, ct), cancellationToken);
        }

        private async Task InternalCreateAsync(TId id, TModel item, CancellationToken cancellationToken)
        {
            ValidateNotExists(id);
            await VerifyUniqueForCreateAsync(item, cancellationToken);

            item.TrySetPrimaryKey(id);
            var success = MemoryItems.TryAdd(id, item);
            if (!success) throw new FulcrumConflictException($"Item with id {id} already exists.");
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await CreateWithSpecifiedIdAsync(id, item, cancellationToken);
            return await ReadAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            var itemCopy = GetMemoryItem(id, true);
            return Task.FromResult(itemCopy);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            try
            {
                await _semaphore.RaiseAsync(cancellationToken);
                var list = MemoryItems.Keys
                    .Select(id => GetMemoryItem(id, true))
                    .Where(item => item != null)
                    .Skip(offset)
                    .Take(limit.Value)
                    .ToList();
                var page = new PageEnvelope<TModel>(offset, limit.Value, MemoryItems.Count, list);
                return page;
            }
            finally
            {
                _semaphore.Lower();
            }
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TModel>> ReadAllAsync(int limit = Int32.MaxValue,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            try
            {
                await _semaphore.RaiseAsync(cancellationToken);
                var list = MemoryItems.Keys
                    .Select(id => GetMemoryItem(id, true))
                    .Where(item => item != null)
                    .Take(limit)
                    .ToList();
                return list;
            }
            finally
            {
                _semaphore.Lower();
            }
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset = 0, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            try
            {
                await _semaphore.RaiseAsync(cancellationToken);
                var list = SearchHelper.FilterAndSort(MemoryItems.Values, details)
                    .Skip(offset)
                    .Take(limit.Value)
                    .Select(item => CopyItem(item))
                    .ToList();
                var page = new PageEnvelope<TModel>(offset, limit.Value, MemoryItems.Count, list);
                return page;
            }
            finally
            {
                _semaphore.Lower();
            }
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TId id, TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (!Exists(id)) throw new FulcrumNotFoundException($"Update failed. Could not find an item with id {id}.");

            var oldValue = MaybeVerifyEtagForUpdate(id, item, cancellationToken);
            var itemCopy = CopyItem(item);
            StorageHelper.MaybeUpdateTimeStamps(itemCopy, false);
            itemCopy.TrySetOptimisticConcurrencyControl();
            await _semaphore.ExecuteAsync(async () =>
            {
                await VerifyUniqueForUpdateAsync(id, item, cancellationToken);
                MemoryItems[id] = itemCopy;
            }, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await UpdateAsync(id, item, cancellationToken);
            return await ReadAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Idempotent, i.e. will not throw an exception if the item does not exist.
        /// </remarks>
        public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            try
            {
                await _semaphore.RaiseAsync(cancellationToken);
                MemoryItems.TryRemove(id, out var _);
            }
            finally
            {
                _semaphore.Lower();
            }
        }

        /// <inheritdoc />
        public virtual async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.RaiseAsync(cancellationToken);
                MemoryItems.Clear();
            }
            finally
            {
                _semaphore.Lower();
            }
        }

        /// <inheritdoc />
        public virtual Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            var key = TypeConversionExtensions.MapToType<string, TId>(id);
            var newLock = new Lock<TId>
            {
                LockId = CreateNewId<TId>(),
                ItemId = id,
                ValidUntil = DateTimeOffset.Now.AddSeconds(30)
            };
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Locks.TryAdd(id, newLock)) return Task.FromResult(newLock);
                if (!Locks.TryGetValue(id, out var oldLock)) continue;
                var remainingTime = oldLock.ValidUntil.Subtract(DateTimeOffset.Now);
                if (remainingTime > TimeSpan.Zero)
                {
                    var message = $"Item {key} is locked by someone else. The lock will be released before {oldLock.ValidUntil}";
                    var exception = new FulcrumResourceLockedException(message)
                    {
                        RecommendedWaitTimeInSeconds = remainingTime.Seconds
                    };
                    throw exception;
                }
                if (Locks.TryUpdate(id, newLock, oldLock)) return Task.FromResult(newLock);
            }
        }

        /// <inheritdoc />
        public virtual Task ReleaseLockAsync(TId id, TId lockId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(lockId, nameof(lockId));
            if (!Locks.TryGetValue(id, out var @lock)) return Task.CompletedTask;
            if (!Equals(lockId, @lock.LockId)) return Task.CompletedTask;
            // Try to temporarily add additional time to make sure that nobody steals the lock while we are releasing it.
            // The TryUpdate will return false if there is no lock or if the current lock differs from the lock we want to release.
            var newLock = new Lock<TId>
            {
                LockId = lockId,
                ItemId = id,
                ValidUntil = DateTimeOffset.Now.AddSeconds(30)
            };
            if (!Locks.TryUpdate(id, @lock, newLock)) return Task.CompletedTask;
            // Finally remove the lock
            Locks.TryRemove(id, out var _);
            return Task.CompletedTask;
        }

        #region private

        private void ValidateNotExists(TId id)
        {
            if (!Exists(id)) return;
            throw new FulcrumConflictException(
                $"An item of type {typeof(TModel).Name} with id \"{id}\" already exists.");
        }

        private static TModel CopyItem(TModelCreate source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var itemCopy = StorageHelper.DeepCopy<TModel, TModelCreate>(source);
            if (itemCopy == null)
                throw new FulcrumAssertionFailedException("Could not copy an item.");
            return itemCopy;
        }
        #endregion

        /// <inheritdoc />
        public virtual Task<Lock<TId>> ClaimDistributedLockAsync(TId id, TimeSpan? lockTimeSpan = null,
            TId currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            lockTimeSpan = lockTimeSpan ?? TimeSpan.FromSeconds(30);
            var key = TypeConversionExtensions.MapToType<string, TId>(id);
            var newLockId = TypeConversionExtensions.MapToType<TId, Guid>(Guid.NewGuid());
            while (true)
            {
                var newLock = new Lock<TId>
                {
                    LockId = newLockId,
                    ItemId = id,
                    ValidUntil = DateTimeOffset.Now.Add(lockTimeSpan.Value)
                };
                cancellationToken.ThrowIfCancellationRequested();
                if (Locks.TryAdd(id, newLock)) return Task.FromResult(newLock);
                if (!Locks.TryGetValue(id, out var oldLock)) continue;
                var remainingTime = oldLock.ValidUntil.Subtract(DateTimeOffset.Now);
                if (remainingTime > TimeSpan.Zero && !Equals(oldLock.LockId, currentLockId))
                {
                    var message = $"Item {key} is locked by someone else. The lock will be released before {oldLock.ValidUntil}";
                    var exception = new FulcrumResourceLockedException(message)
                    {
                        RecommendedWaitTimeInSeconds = remainingTime.Seconds
                    };
                    throw exception;
                }

                newLock.LockId = Equals(currentLockId, oldLock.LockId) ? currentLockId : newLockId;
                if (Locks.TryUpdate(id, newLock, oldLock)) return Task.FromResult(newLock);
            }
        }

        /// <inheritdoc />
        public virtual Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(lockId, nameof(lockId));
            if (!Locks.TryGetValue(id, out Lock<TId> @lock)) return Task.CompletedTask;
            if (!Equals(lockId, @lock.LockId)) return Task.CompletedTask;
            // Try to temporarily add additional time to make sure that nobody steals the lock while we are releasing it.
            // The TryUpdate will return false if there is no lock or if the current lock differs from the lock we want to release.
            var newLock = new Lock<TId>
            {
                LockId = lockId,
                ItemId = id,
                ValidUntil = DateTimeOffset.Now.AddSeconds(30)
            };
            if (!Locks.TryUpdate(id, @lock, newLock)) return Task.CompletedTask;
            // Finally remove the lock
            Locks.TryRemove(id, out var _);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task ClaimTransactionLockAsync(TId id, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken cancellationToken = default)
        {
            return ReadAsync(id, cancellationToken);
        }

        private async Task VerifyUniqueForCreateAsync(TModelCreate item, CancellationToken cancellationToken)
        {
            if (UniqueConstraintMethods == null) return;
            foreach (var method in UniqueConstraintMethods.GetInvocationList())
            {
                var getSearchObjectMethod = method as UniqueConstraintDelegate;
                if (getSearchObjectMethod == null) continue;
                var where = getSearchObjectMethod(item);
                var page = await SearchAsync(new SearchDetails<TModel>(where), 0, 1, cancellationToken);
                if (page != null && page.PageInfo.Returned > 0)
                {
                    throw new FulcrumConflictException($"The new item of type {typeof(TModelCreate).Name} must be unique.");
                }
            }
        }

        private async Task VerifyUniqueForUpdateAsync(TId id, TModelCreate item, CancellationToken cancellationToken)
        {
            if (UniqueConstraintMethods == null) return;
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            foreach (var method in UniqueConstraintMethods.GetInvocationList())
            {
                var getSearchObjectMethod = method as UniqueConstraintDelegate;
                if (getSearchObjectMethod == null) continue;
                var where = getSearchObjectMethod(item);
                var page = await SearchAsync(new SearchDetails<TModel>(where), 0, 1, cancellationToken);
                if (page == null || page.PageInfo.Returned <= 0) continue;
                var foundItem = page.Data.First();
                FulcrumAssert.IsNotNull(foundItem, CodeLocation.AsString());
                if (!foundItem.TryGetPrimaryKey<TModel, TId>(out var foundId)) continue;
                if (foundId.Equals(id)) continue;
                throw new FulcrumConflictException($"The new item of type {typeof(TModelCreate).Name} must be unique, but an item with primary key = {id} already exists.");
            }
        }
    }
}

