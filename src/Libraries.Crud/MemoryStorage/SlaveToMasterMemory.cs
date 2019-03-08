using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
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


        /// <inheritdoc />
        public Task<TId> CreateAsync(TId masterId, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TModel> CreateAndReturnAsync(TId masterId, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(TId masterId, TId slaveId, TModelCreate item,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.CreateWithSpecifiedIdAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TId slaveId, TModelCreate item,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.CreateWithSpecifiedIdAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TModel> ReadAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.ReadAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task<TModel> ReadAsync(SlaveToMasterId<TId> id, CancellationToken token = default(CancellationToken))
        {
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId parentId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }
            var groupPersistance = GetStorage(parentId);
            return groupPersistance.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TModel>> ReadChildrenAsync(TId masterId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.ReadAllAsync(limit, token);
        }

        /// <inheritdoc />
        public Task UpdateAsync(TId masterId, TId slaveId, TModel item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.UpdateAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TModel> UpdateAndReturnAsync(TId masterId, TId slaveId, TModel item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.UpdateAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.DeleteAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(TId masterId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            var groupPersistance = GetStorage(masterId);
            return groupPersistance.DeleteAllAsync(token);
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
        public async Task<SlaveLock<TId>> ClaimLockAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
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
        public Task ReleaseLockAsync(TId masterId, TId slaveId, TId lockId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var groupPersistence = GetStorage(masterId);
            return groupPersistence.ReleaseLockAsync(slaveId, lockId, token);
        }
    }
}