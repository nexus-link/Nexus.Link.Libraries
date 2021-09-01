﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{
    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    [Obsolete("Use DependentToMasterMemory. Obsolete since 2021-08-27.")]
    public class DependentToMasterMemory<TModel, TId, TDependentId> :
        DependentToMasterMemory<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMaster<TModel, TId, TDependentId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    [Obsolete("Use DependentToMasterMemory. Obsolete since 2021-08-27.")]
    public class DependentToMasterMemory<TModelCreate, TModel, TId, TDependentId> :
        MemoryBase<TModel, TDependentId>,
        ICrudDependentToMaster<TModelCreate, TModel, TId, TDependentId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// The storages; One dictionary with a memory storage for each master id.
        /// </summary>
        protected static readonly ConcurrentDictionary<TId, CrudMemory<TModelCreate, TModel, TDependentId>> Storages = new ConcurrentDictionary<TId, CrudMemory<TModelCreate, TModel, TDependentId>>();

        private readonly DependentToMasterConvenience<TModelCreate, TModel, TId, TDependentId> _convenience;

        public DependentToMasterMemory()
        {
            _convenience = new DependentToMasterConvenience<TModelCreate, TModel, TId, TDependentId>(this);
        }

        /// <inheritdoc />
        public virtual async Task<TDependentId> CreateAsync(TId masterId, TModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var masterPersistence = GetStorage(masterId);
            var dependentId = await masterPersistence.CreateAsync(item, token);
            FulcrumAssert.IsTrue(masterPersistence.MemoryItems.ContainsKey(dependentId), CodeLocation.AsString());
            var memoryItem = masterPersistence.MemoryItems[dependentId];
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, memoryItem);
            if (memoryItem is IUniquelyIdentifiable<TId> uniqueId)
            {
                uniqueId.Id = StorageHelper.CreateNewId<TId>();
            }

            return dependentId;
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateAndReturnAsync(TId masterId, TModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var masterPersistence = GetStorage(masterId);
            var dependentId = await masterPersistence.CreateAsync(item, token);
            FulcrumAssert.IsTrue(masterPersistence.MemoryItems.ContainsKey(dependentId), CodeLocation.AsString());
            var memoryItem = masterPersistence.MemoryItems[dependentId];
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, memoryItem);
            if (memoryItem is IUniquelyIdentifiable<TId> uniqueId)
            {
                uniqueId.Id = StorageHelper.CreateNewId<TId>();
            }

            return await ReadAsync(masterId, dependentId, token);
        }

        /// <inheritdoc />
        public virtual async Task CreateWithSpecifiedIdAsync(TId masterId, TDependentId dependentId, TModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var masterPersistence = GetStorage(masterId);
            await masterPersistence.CreateWithSpecifiedIdAsync(dependentId, item, token);
            FulcrumAssert.IsTrue(masterPersistence.MemoryItems.ContainsKey(dependentId), CodeLocation.AsString());
            var memoryItem = masterPersistence.MemoryItems[dependentId];
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, memoryItem);
            if (memoryItem is IUniquelyIdentifiable<TId> uniqueId)
            {
                uniqueId.Id = StorageHelper.CreateNewId<TId>();
            }
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TDependentId dependentId, TModelCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var masterPersistence = GetStorage(masterId);
            await masterPersistence.CreateWithSpecifiedIdAsync(dependentId, item, token);
            FulcrumAssert.IsTrue(masterPersistence.MemoryItems.ContainsKey(dependentId), CodeLocation.AsString());
            var memoryItem = masterPersistence.MemoryItems[dependentId];
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, memoryItem);
            if (memoryItem is IUniquelyIdentifiable<TId> uniqueId)
            {
                uniqueId.Id = StorageHelper.CreateNewId<TId>();
            }

            return await ReadAsync(masterId, dependentId, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAsync(dependentId, token);
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId masterId, int offset, int? limit = null, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadChildrenAsync(TId masterId, int limit = int.MaxValue, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAllAsync(limit, token);
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (item is IUniquelyIdentifiableDependent<TId, TDependentId> combinedId)
            {
                InternalContract.RequireAreEqual(masterId, combinedId.MasterId, $"{nameof(item)}.{nameof(combinedId.MasterId)}");
                InternalContract.RequireAreEqual(dependentId, combinedId.DependentId, $"{nameof(item)}.{nameof(combinedId.DependentId)}");
            }
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.UpdateAsync(dependentId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> UpdateAndReturnAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (item is IUniquelyIdentifiableDependent<TId, TDependentId> combinedId)
            {
                InternalContract.RequireAreEqual(masterId, combinedId.MasterId, $"{nameof(item)}.{nameof(combinedId.MasterId)}");
                InternalContract.RequireAreEqual(dependentId, combinedId.DependentId, $"{nameof(item)}.{nameof(combinedId.DependentId)}");
            }
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.UpdateAndReturnAsync(dependentId, item, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.DeleteAsync(dependentId, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TId masterId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.DeleteAllAsync(token);
        }

        #region private
        /// <summary>
        ///  Get the storage for a specific <paramref name="masterId"/>.
        /// </summary>
        /// <param name="masterId"></param>
        /// <returns></returns>
        private static CrudMemory<TModelCreate, TModel, TDependentId> GetStorage(TId masterId)
        {
            lock (Storages)
            {
                if (!Storages.ContainsKey(masterId))
                    Storages[masterId] = new CrudMemory<TModelCreate, TModel, TDependentId>();
                return Storages[masterId];
            }
        }
        #endregion

        private readonly ConcurrentDictionary<TId, Lock<TDependentId>> _groupLocks = new ConcurrentDictionary<TId, Lock<TDependentId>>();

        /// <inheritdoc />
        public virtual async Task<DependentLock<TId, TDependentId>> ClaimDistributedLockAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            var groupLock = await groupPersistence.ClaimLockAsync(dependentId, token);
            var dependentLock = new DependentLock<TId, TDependentId>
            {
                Id = groupPersistence.CreateNewId<TId>(),
                MasterId = masterId,
                DependentId = groupLock.ItemId,
                ValidUntil = groupLock.ValidUntil
            };

            _groupLocks.TryAdd(dependentLock.Id, groupLock);
            return dependentLock;
        }

        /// <inheritdoc />
        public virtual async Task ReleaseDistributedLockAsync(TId masterId, TDependentId dependentId, TId lockId,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            if (!_groupLocks.TryGetValue(lockId, out var groupLock)) return;
            await groupPersistence.ReleaseLockAsync(dependentId, groupLock.Id, token);
        }

        /// <inheritdoc />
        public virtual Task ClaimTransactionLockAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            return _convenience.ClaimTransactionLockAndReadAsync(masterId, dependentId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(TId parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            return _convenience.SearchChildrenAsync(parentId, details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(TId parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TId> GetDependentUniqueIdAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            var item = await ReadAsync(masterId, dependentId, token);
            if (!(item is IUniquelyIdentifiable<TId> uniquelyIdentifiable))
            {
                throw new FulcrumNotFoundException($"Could not find a dependent object of type {typeof(TModel).Name} with master id {masterId} and dependent id {dependentId}.");
            }

            return uniquelyIdentifiable.Id;
        }
    }
}