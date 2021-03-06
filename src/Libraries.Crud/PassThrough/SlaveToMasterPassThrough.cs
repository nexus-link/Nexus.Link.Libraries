﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.PassThrough
{
    /// <inheritdoc cref="SlaveToMasterPassThrough{TModel,TId}" />
    public class SlaveToMasterPassThrough<TModel, TId> :
        SlaveToMasterPassThrough<TModel, TModel, TId>,
        ICrudSlaveToMaster<TModel, TId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public SlaveToMasterPassThrough(ICrudable<TModel, TId> service)
            : base(service)
        {
        }
    }

    /// <inheritdoc cref="ICrudManyToOne{TModelCreate,TModel,TId}" />
    public class SlaveToMasterPassThrough<TModelCreate, TModel, TId> :
        ICrudSlaveToMaster<TModelCreate, TModel, TId>
         where TModel : TModelCreate
    {
        /// <summary>
        /// The service to pass the calls to.
        /// </summary>
        protected readonly ICrudable<TModel, TId> Service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public SlaveToMasterPassThrough(ICrudable<TModel, TId> service)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            Service = service;
        }

        /// <inheritdoc />
        public virtual Task<TId> CreateAsync(TId masterId, TModelCreate item, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateSlave<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateAsync(masterId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateAndReturnAsync(TId masterId, TModelCreate item, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateSlaveAndReturn<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateAndReturnAsync(masterId, item, token);
        }

        /// <inheritdoc />
        public virtual Task CreateWithSpecifiedIdAsync(TId masterId, TId slaveId, TModelCreate item,
            CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateSlaveWithSpecifiedId<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateWithSpecifiedIdAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TId slaveId, TModelCreate item,
            CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateSlaveWithSpecifiedId<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateWithSpecifiedIdAndReturnAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId masterId, TId slaveId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadSlave<TModel, TId>>(Service);
            return implementation.ReadAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public Task<TModel> ReadAsync(SlaveToMasterId<TId> id, CancellationToken token = default)
        {
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId parentId, int offset, int? limit = null,
            CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadChildrenWithPaging<TModel, TId>>(Service);
            return implementation.ReadChildrenWithPagingAsync(parentId, offset, limit, token);
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadChildrenAsync(TId parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadChildren<TModel, TId>>(Service);
            return implementation.ReadChildrenAsync(parentId, limit, token);
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TId masterId, TId slaveId, TModel item, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdateSlave<TModel, TId>>(Service);
            return implementation.UpdateAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> UpdateAndReturnAsync(TId masterId, TId slaveId, TModel item, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdateSlaveAndReturn<TModel, TId>>(Service);
            return implementation.UpdateAndReturnAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TId masterId, TId slaveId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDeleteSlave<TId>>(Service);
            return implementation.DeleteAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TId parentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDeleteChildren<TId>>(Service);
            return implementation.DeleteChildrenAsync(parentId, token);
        }

        /// <inheritdoc />
        public virtual Task<SlaveLock<TId>> ClaimLockAsync(TId masterId, TId slaveId, CancellationToken token = default)
        {
#pragma warning disable 618
            var implementation = CrudHelper.GetImplementationOrThrow<ILockableSlave<TId>>(Service);
#pragma warning restore 618
            return implementation.ClaimLockAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public virtual Task ReleaseLockAsync(TId masterId, TId slaveId, TId lockId, CancellationToken token = default)
        {
#pragma warning disable 618
            var implementation = CrudHelper.GetImplementationOrThrow<ILockableSlave<TId>>(Service);
#pragma warning restore 618
            return implementation.ReleaseLockAsync(masterId, slaveId, lockId, token);
        }

        /// <inheritdoc />
        public virtual Task<SlaveLock<TId>> ClaimDistributedLockAsync(TId masterId, TId slaveId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDistributedLockSlave<TId>>(Service);
            return implementation.ClaimDistributedLockAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public virtual Task ReleaseDistributedLockAsync(TId masterId, TId slaveId, TId lockId,
            CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDistributedLockSlave<TId>>(Service);
            return implementation.ReleaseDistributedLockAsync(masterId, slaveId, lockId, token);
        }

        /// <inheritdoc />
        public virtual Task ClaimTransactionLockAsync(TId masterId, TId slaveId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ITransactionLockSlave<TModel, TId>>(Service);
            return implementation.ClaimTransactionLockAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId masterId, TId slaveId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ITransactionLockSlave<TModel, TId>>(Service);
            return implementation.ClaimTransactionLockAndReadAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(TId parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ISearchChildren<TModel,TId>>(Service);
            return implementation.SearchChildrenAsync(parentId, details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(TId parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ISearchChildren<TModel,TId>>(Service);
            return implementation.FindUniqueChildAsync(parentId, details, cancellationToken);
        }
    }
}
