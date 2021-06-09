using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{
    /// <summary>
    /// General class for storing any <see cref="IUniquelyIdentifiable{TId}"/> in memory.
    /// </summary>
    /// <typeparam name="TModel">The type of objects that are returned from persistant storage.</typeparam>
    /// <typeparam name="TId"></typeparam>
    public class CrudMemory<TModel, TId> :
        CrudMemory<TModel, TModel, TId>,
        ICrud<TModel, TId>
    {
    }

    /// <summary>
    /// General class for storing any <see cref="IUniquelyIdentifiable{TId}"/> in memory.
    /// </summary>
    /// <typeparam name="TModelCreate">The type for creating objects in persistant storage.</typeparam>
    /// <typeparam name="TModel">The type of objects that are returned from persistant storage.</typeparam>
    /// <typeparam name="TId"></typeparam>
    public class CrudMemory<TModelCreate, TModel, TId> :
        MemoryBase<TModel, TId>,
        ICrud<TModelCreate, TModel, TId>, ISearch<TModel, TId> where TModel : TModelCreate
    {
        private readonly CrudConvenience<TModelCreate, TModel, TId> _convenience;

        public CrudMemory()
        {
            _convenience = new CrudConvenience<TModelCreate, TModel, TId>(this);
        }

        /// <inheritdoc />
        public virtual async Task<TId> CreateAsync(TModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var id = StorageHelper.CreateNewId<TId>();
            await CreateWithSpecifiedIdAsync(id, item, token);
            return id;
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var id = await CreateAsync(item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        public virtual async Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var itemCopy = CopyItem(item);

            StorageHelper.MaybeCreateNewEtag(itemCopy);
            StorageHelper.MaybeUpdateTimeStamps(itemCopy, true);
            lock (MemoryItems)
            {
                ValidateNotExists(id);
                StorageHelper.MaybeSetId(id, itemCopy);
                var success = MemoryItems.TryAdd(id, itemCopy);
                if (!success) throw new FulcrumConflictException($"Item with id {id} already exists.");
            }
            await Task.Yield();
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await CreateWithSpecifiedIdAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            var itemCopy = GetMemoryItem(id, true);
            return Task.FromResult(itemCopy);
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            lock (MemoryItems)
            {
                var list = MemoryItems.Keys
                    .Select(id => GetMemoryItem(id, true))
                    .Where(item => item != null)
                    .Skip(offset)
                    .Take(limit.Value)
                    .ToList();
                var page = new PageEnvelope<TModel>(offset, limit.Value, MemoryItems.Count, list);
                return Task.FromResult(page);
            }
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadAllAsync(int limit = Int32.MaxValue,
            CancellationToken token = default)
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            lock (MemoryItems)
            {
                var list = MemoryItems.Keys
                    .Select(id => GetMemoryItem(id, true))
                    .Where(item => item != null)
                    .Take(limit)
                    .ToList();
                return Task.FromResult((IEnumerable<TModel>)list);
            }
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset = 0, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            lock (MemoryItems)
            {
                var list = SearchHelper.FilterAndSort(MemoryItems.Values, details)
                    .Skip(offset)
                    .Take(limit.Value)
                    .Select(item => CopyItem(item))
                    .ToList();
                var page = new PageEnvelope<TModel>(offset, limit.Value, MemoryItems.Count, list);
                return Task.FromResult(page);
            }
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TId id, TModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (!Exists(id)) throw new FulcrumNotFoundException($"Update failed. Could not find an item with id {id}.");

            var oldValue = await MaybeVerifyEtagForUpdateAsync(id, item, this, token);
            var itemCopy = CopyItem(item);
            StorageHelper.MaybeUpdateTimeStamps(itemCopy, false);
            StorageHelper.MaybeCreateNewEtag(itemCopy);

            MemoryItems[id] = itemCopy;
        }

        /// <inheritdoc />
        public virtual async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await UpdateAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Idempotent, i.e. will not throw an exception if the item does not exist.
        /// </remarks>
        public virtual Task DeleteAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            MemoryItems.TryRemove(id, out var _);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task DeleteAllAsync(CancellationToken token = default)
        {
            lock (MemoryItems)
            {
                MemoryItems.Clear();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            var key = MapperHelper.MapToType<string, TId>(id);
            var newLock = new Lock<TId>
            {
                Id = MapperHelper.MapToType<TId, Guid>(Guid.NewGuid()),
                ItemId = id,
                ValidUntil = DateTimeOffset.Now.AddSeconds(30)
            };
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (_locks.TryAdd(id, newLock)) return Task.FromResult(newLock);
                if (!_locks.TryGetValue(id, out var oldLock)) continue;
                var remainingTime = oldLock.ValidUntil.Subtract(DateTimeOffset.Now);
                if (remainingTime > TimeSpan.Zero)
                {
                    var message = $"Item {key} is locked by someone else. The lock will be released before {oldLock.ValidUntil}";
                    var exception = new FulcrumTryAgainException(message)
                    {
                        RecommendedWaitTimeInSeconds = remainingTime.Seconds
                    };
                    throw exception;
                }
                if (_locks.TryUpdate(id, newLock, oldLock)) return Task.FromResult(newLock);
            }
        }

        /// <inheritdoc />
        public virtual Task ReleaseLockAsync(TId id, TId lockId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(lockId, nameof(lockId));
            if (!_locks.TryGetValue(id, out Lock<TId> @lock)) return Task.CompletedTask;
            if (!Equals(lockId, @lock.Id)) return Task.CompletedTask;
            // Try to temporarily add additional time to make sure that nobody steals the lock while we are releasing it.
            // The TryUpdate will return false if there is no lock or if the current lock differs from the lock we want to release.
            var newLock = new Lock<TId>
            {
                Id = lockId,
                ItemId = id,
                ValidUntil = DateTimeOffset.Now.AddSeconds(30)
            };
            if (!_locks.TryUpdate(id, @lock, newLock)) return Task.CompletedTask;
            // Finally remove the lock
            _locks.TryRemove(id, out var _);
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
        public virtual Task<Lock<TId>> ClaimDistributedLockAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            var key = MapperHelper.MapToType<string, TId>(id);
            var newLock = new Lock<TId>
            {
                Id = MapperHelper.MapToType<TId, Guid>(Guid.NewGuid()),
                ItemId = id,
                ValidUntil = DateTimeOffset.Now.AddSeconds(30)
            };
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (_locks.TryAdd(id, newLock)) return Task.FromResult(newLock);
                if (!_locks.TryGetValue(id, out var oldLock)) continue;
                var remainingTime = oldLock.ValidUntil.Subtract(DateTimeOffset.Now);
                if (remainingTime > TimeSpan.Zero)
                {
                    var message = $"Item {key} is locked by someone else. The lock will be released before {oldLock.ValidUntil}";
                    var exception = new FulcrumTryAgainException(message)
                    {
                        RecommendedWaitTimeInSeconds = remainingTime.Seconds
                    };
                    throw exception;
                }
                if (_locks.TryUpdate(id, newLock, oldLock)) return Task.FromResult(newLock);
            }
        }

        /// <inheritdoc />
        public virtual Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotDefaultValue(lockId, nameof(lockId));
            if (!_locks.TryGetValue(id, out Lock<TId> @lock)) return Task.CompletedTask;
            if (!Equals(lockId, @lock.Id)) return Task.CompletedTask;
            // Try to temporarily add additional time to make sure that nobody steals the lock while we are releasing it.
            // The TryUpdate will return false if there is no lock or if the current lock differs from the lock we want to release.
            var newLock = new Lock<TId>
            {
                Id = lockId,
                ItemId = id,
                ValidUntil = DateTimeOffset.Now.AddSeconds(30)
            };
            if (!_locks.TryUpdate(id, @lock, newLock)) return Task.CompletedTask;
            // Finally remove the lock
            _locks.TryRemove(id, out var _);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task ClaimTransactionLockAsync(TId id, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken token = default)
        {
            return ReadAsync(id, token);
        }
    }
}

