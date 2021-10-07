using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Helpers
{
    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class DependentToMasterWithUniqueIdConvenience<TModel, TId, TDependentId> :
        DependentToMasterWithUniqueIdConvenience<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMaster<TModel, TId, TDependentId>
        where TModel : IUniquelyIdentifiableDependent<TId, TDependentId>, IUniquelyIdentifiable<TId>
    {
        /// <inheritdoc />
        public DependentToMasterWithUniqueIdConvenience(ICrudManyToOne<TModel, TId> uniqueIdTable) : base(uniqueIdTable)
        {
        }
    }

    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class DependentToMasterWithUniqueIdConvenience<TModelCreate, TModel, TId, TDependentId> :
        ICrudDependentToMasterWithUniqueId<TModelCreate, TModel, TId, TDependentId>
        where TModel : TModelCreate, IUniquelyIdentifiableDependent<TId, TDependentId>
        where TModelCreate : IUniquelyIdentifiable<TId>
    {
        private readonly ICrudManyToOne<TModelCreate, TModel, TId> _uniqueIdTable;

        private readonly DependentToMasterConvenience<TModelCreate, TModel, TId, TDependentId> _convenience;

        public DependentToMasterWithUniqueIdConvenience(ICrudManyToOne<TModelCreate, TModel, TId> uniqueIdTable)
        {
            _uniqueIdTable = uniqueIdTable;
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
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, item);
            await _uniqueIdTable.CreateAndReturnAsync(item, cancellationToken );
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
            StorageHelper.MaybeSetMasterAndDependentId(masterId, dependentId, item);
            return await _uniqueIdTable.CreateAndReturnAsync(item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<TModel> ReadAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            return await _uniqueIdTable.FindUniqueAsync(new SearchDetails<TModel>(new
                {MasterId = masterId, DependentId = dependentId}), cancellationToken );
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
            return _uniqueIdTable.ReadChildrenWithPagingAsync(masterId, offset, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadChildrenAsync(TId masterId, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return _uniqueIdTable.ReadChildrenAsync(masterId, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, cancellationToken );
            await _uniqueIdTable.UpdateAsync(uniqueId, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<TModel> UpdateAndReturnAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, cancellationToken );
            return await _uniqueIdTable.UpdateAndReturnAsync(uniqueId, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, cancellationToken );
            await _uniqueIdTable.DeleteAsync(uniqueId, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TId masterId, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            return _uniqueIdTable.DeleteChildrenAsync(masterId, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<DependentLock<TId, TDependentId>> ClaimDistributedLockAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, cancellationToken );
            var distributedLock = await _uniqueIdTable.ClaimDistributedLockAsync(uniqueId, cancellationToken );
            var dependentLock = new DependentLock<TId, TDependentId>
            {
                Id = distributedLock.Id,
                MasterId = masterId,
                DependentId = dependentId,
                ValidUntil = distributedLock.ValidUntil
            };

            return dependentLock;
        }

        /// <inheritdoc />
        public virtual async Task ReleaseDistributedLockAsync(TId masterId, TDependentId dependentId, TId lockId,
            CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, cancellationToken );
            await _uniqueIdTable.ReleaseLockAsync(uniqueId, lockId, cancellationToken );
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

        /// <inheritdoc />
        public Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _uniqueIdTable.ReadAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task UpdateAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            return _uniqueIdTable.UpdateAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId id, CancellationToken cancellationToken  = default)
        {
            return _uniqueIdTable.DeleteAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            return _uniqueIdTable.UpdateAndReturnAsync(id, item, cancellationToken );
        }
    }
}