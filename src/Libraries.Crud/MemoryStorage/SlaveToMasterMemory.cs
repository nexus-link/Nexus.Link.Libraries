using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{
    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class SlaveToMasterMemory<TModel, TId> : 
        SlaveToMasterMemory<TModel, TModel, TId>, 
        ICrudSlaveToMaster<TModel, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class SlaveToMasterMemory<TModelCreate, TModel, TId> : 
        MemoryBase<TModel, TId>,
        ICrudSlaveToMaster<TModelCreate, TModel, TId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// The storages; One dictionary with a memory storage for each master id.
        /// </summary>
        protected static readonly ConcurrentDictionary<TId, CrudMemory<TModelCreate, TModel, TId>> Storages = new ConcurrentDictionary<TId, CrudMemory<TModelCreate, TModel, TId>>();

        private readonly SlaveToMasterConvenience<TModelCreate, TModel, TId> _convenience;

        public SlaveToMasterMemory()
        {
            _convenience = new SlaveToMasterConvenience<TModelCreate, TModel, TId>(this);
        }


        /// <inheritdoc />
        public virtual Task<TId> CreateAsync(TId masterId, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateAndReturnAsync(TId masterId, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public virtual Task CreateWithSpecifiedIdAsync(TId masterId, TId slaveId, TModelCreate item,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.CreateWithSpecifiedIdAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TId slaveId, TModelCreate item,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.CreateWithSpecifiedIdAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAsync(slaveId, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(SlaveToMasterId<TId> id, CancellationToken token = default(CancellationToken))
        {
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId parentId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }
            var groupPersistence = GetStorage(parentId);
            return groupPersistence.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadChildrenAsync(TId masterId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReadAllAsync(limit, token);
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TId masterId, TId slaveId, TModel item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.UpdateAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> UpdateAndReturnAsync(TId masterId, TId slaveId, TModel item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.UpdateAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.DeleteAsync(slaveId, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TId masterId, CancellationToken token = default(CancellationToken))
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
        private CrudMemory<TModelCreate, TModel, TId> GetStorage(TId masterId)
        {
            if (!Storages.ContainsKey(masterId)) Storages[masterId] = new CrudMemory<TModelCreate, TModel, TId>();
            return Storages[masterId];
        }
        #endregion

        /// <inheritdoc />
        public virtual async Task<SlaveLock<TId>> ClaimLockAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistence = GetStorage(masterId);
            var groupLock = await groupPersistence.ClaimLockAsync(slaveId, token);
            return new SlaveLock<TId>
            {
                Id = groupLock.Id,
                MasterId = masterId,
                SlaveId = groupLock.ItemId,
                ValidUntil = groupLock.ValidUntil
            };
        }

        /// <inheritdoc />
        public virtual Task ReleaseLockAsync(TId masterId, TId slaveId, TId lockId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReleaseLockAsync(slaveId, lockId, token);
        }

        /// <inheritdoc />
        public virtual async Task<SlaveLock<TId>> ClaimDistributedLockAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistence = GetStorage(masterId);
            var groupLock = await groupPersistence.ClaimLockAsync(slaveId, token);
            return new SlaveLock<TId>
            {
                Id = groupLock.Id,
                MasterId = masterId,
                SlaveId = groupLock.ItemId,
                ValidUntil = groupLock.ValidUntil
            };
        }

        /// <inheritdoc />
        public virtual Task ReleaseDistributedLockAsync(TId masterId, TId slaveId, TId lockId,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReleaseLockAsync(slaveId, lockId, token);
        }

        /// <inheritdoc />
        public virtual Task ClaimTransactionLockAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(TId parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _convenience.SearchChildrenAsync(parentId, details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(TId parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }
    }
}