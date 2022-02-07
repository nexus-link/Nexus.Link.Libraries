using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.EntityAttributes;
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
    public class DependentToMasterMemory<TModel, TId, TDependentId> :
        DependentToMasterMemory<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMaster<TModel, TId, TDependentId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
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
        public virtual async Task CreateWithSpecifiedIdAsync(TId masterId, TDependentId dependentId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (item is IUniquelyIdentifiableDependent<TId, TDependentId> combinedId)
            {
                InternalContract.RequireAreEqual(masterId, combinedId.MasterId, $"{nameof(item)}.{nameof(combinedId.MasterId)}");
                InternalContract.RequireAreEqual(dependentId, combinedId.DependentId, $"{nameof(item)}.{nameof(combinedId.DependentId)}");
            }
            var masterPersistence = GetStorage(masterId);
            await masterPersistence.CreateWithSpecifiedIdAsync(dependentId, item, cancellationToken );
            FulcrumAssert.IsTrue(masterPersistence.MemoryItems.ContainsKey(dependentId), CodeLocation.AsString());
            var memoryItem = masterPersistence.MemoryItems[dependentId];
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, memoryItem);
            memoryItem.TrySetPrimaryKey(StorageHelper.CreateNewId<TId>());
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TDependentId dependentId, TModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (item is IUniquelyIdentifiableDependent<TId, TDependentId> combinedId)
            {
                InternalContract.RequireAreEqual(masterId, combinedId.MasterId, $"{nameof(item)}.{nameof(combinedId.MasterId)}");
                InternalContract.RequireAreEqual(dependentId, combinedId.DependentId, $"{nameof(item)}.{nameof(combinedId.DependentId)}");
            }
            var masterPersistence = GetStorage(masterId);
            await masterPersistence.CreateWithSpecifiedIdAsync(dependentId, item, cancellationToken );
            FulcrumAssert.IsTrue(masterPersistence.MemoryItems.ContainsKey(dependentId), CodeLocation.AsString());
            var memoryItem = masterPersistence.MemoryItems[dependentId];
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, memoryItem);
            memoryItem.TrySetPrimaryKey(StorageHelper.CreateNewId<TId>());

            return await ReadAsync(masterId, dependentId, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAsync(dependentId, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId masterId, int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAllWithPagingAsync(offset, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadChildrenAsync(TId masterId, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAllAsync(limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken cancellationToken  = default)
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
            if (item.TryGetPrimaryKey<TModel, TId>(out var primaryKey))
            {
                var oldItem = await groupPersistence.ReadAsync(dependentId, cancellationToken);
                if (item.TryGetPrimaryKey<TModel, TId>(out var oldPrimaryKey))
                {
                    InternalContract.RequireAreEqual(oldPrimaryKey, primaryKey,
                        $"{nameof(item)}.{item.GetPrimaryKeyPropertyName()}");
                }
            }
            await groupPersistence.UpdateAsync(dependentId, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<TModel> UpdateAndReturnAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken cancellationToken  = default)
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
            if (item.TryGetPrimaryKey<TModel, TId>(out var primaryKey))
            {
                var oldItem = await groupPersistence.ReadAsync(dependentId, cancellationToken );
                if (item.TryGetPrimaryKey<TModel, TId>(out var oldPrimaryKey))
                {
                    InternalContract.RequireAreEqual(oldPrimaryKey, primaryKey,
                        $"{nameof(item)}.{item.GetPrimaryKeyPropertyName()}");
                }
            }
            return await groupPersistence.UpdateAndReturnAsync(dependentId, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.DeleteAsync(dependentId, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TId masterId, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.DeleteAllAsync(cancellationToken );
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
        public virtual async Task<DependentLock<TId, TDependentId>> ClaimDistributedLockAsync(TId masterId,
            TDependentId dependentId, TId currentLockId = default, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            var groupLock = await groupPersistence.ClaimLockAsync(dependentId, cancellationToken );
            var dependentLock = new DependentLock<TId, TDependentId>
            {
                LockId = groupPersistence.CreateNewId<TId>(),
                MasterId = masterId,
                DependentId = groupLock.ItemId,
                ValidUntil = groupLock.ValidUntil
            };

            _groupLocks.TryAdd(dependentLock.LockId, groupLock);
            return dependentLock;
        }

        /// <inheritdoc />
        public virtual async Task ReleaseDistributedLockAsync(TId masterId, TDependentId dependentId, TId lockId,
            CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var groupPersistence = GetStorage(masterId);
            if (!_groupLocks.TryGetValue(lockId, out var groupLock)) return;
            await groupPersistence.ReleaseLockAsync(dependentId, groupLock.LockId, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task ClaimTransactionLockAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            return _convenience.ClaimTransactionLockAndReadAsync(masterId, dependentId, cancellationToken );
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
        public Task<TId> GetDependentUniqueIdAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            return _convenience.GetDependentUniqueIdAsync(masterId, dependentId, cancellationToken );
        }
    }
}