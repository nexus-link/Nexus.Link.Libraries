using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.PassThrough
{
    /// <inheritdoc cref="DependentToMasterPassThrough{TModel,TId, TDependentId}" />
    public class DependentToMasterPassThrough<TModel, TId, TDependentId> :
        DependentToMasterPassThrough<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMaster<TModel, TId, TDependentId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public DependentToMasterPassThrough(ICrudableDependent<TModel, TId, TDependentId> service)
            : base(service)
        {
        }
    }

    /// <inheritdoc cref="ICrudManyToOne{TModelCreate,TModel,TId}" />
    public class DependentToMasterPassThrough<TModelCreate, TModel, TId, TDependentId> :
        ICrudDependentToMaster<TModelCreate, TModel, TId, TDependentId>
         where TModel : TModelCreate
    {
        /// <summary>
        /// The service to pass the calls to.
        /// </summary>
        protected readonly ICrudableDependent<TModel, TId, TDependentId> Service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public DependentToMasterPassThrough(ICrudableDependent<TModel, TId, TDependentId> service)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            Service = service;
        }

        /// <inheritdoc />
        public virtual Task CreateWithSpecifiedIdAsync(TId masterId, TDependentId dependentId, TModelCreate item,
            CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateDependentWithSpecifiedId<TModelCreate, TModel, TId, TDependentId>>(Service);
            return implementation.CreateWithSpecifiedIdAsync(masterId, dependentId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TDependentId dependentId, TModelCreate item,
            CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateDependentWithSpecifiedIdAndReturn<TModelCreate, TModel, TId, TDependentId>>(Service);
            return implementation.CreateWithSpecifiedIdAndReturnAsync(masterId, dependentId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadDependent<TModel, TId, TDependentId>>(Service);
            return implementation.ReadAsync(masterId, dependentId, token);
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
        public virtual Task UpdateAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdateDependent<TModel, TId, TDependentId>>(Service);
            return implementation.UpdateAsync(masterId, dependentId, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> UpdateAndReturnAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdateDependentAndReturn<TModel, TId, TDependentId>>(Service);
            return implementation.UpdateAndReturnAsync(masterId, dependentId, item, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDeleteDependent<TId, TDependentId>>(Service);
            return implementation.DeleteAsync(masterId, dependentId, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TId parentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDeleteChildren<TId>>(Service);
            return implementation.DeleteChildrenAsync(parentId, token);
        }

        /// <inheritdoc />
        public virtual Task<DependentLock<TId, TDependentId>> ClaimDistributedLockAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDependentDistributedLock<TId, TDependentId>>(Service);
            return implementation.ClaimDistributedLockAsync(masterId, dependentId, token);
        }

        /// <inheritdoc />
        public virtual Task ReleaseDistributedLockAsync(TId masterId, TDependentId dependentId, TId lockId,
            CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDependentDistributedLock<TId, TDependentId>>(Service);
            return implementation.ReleaseDistributedLockAsync(masterId, dependentId, lockId, token);
        }

        /// <inheritdoc />
        public virtual Task ClaimTransactionLockAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ITransactionLockDependent<TModel, TId, TDependentId>>(Service);
            return implementation.ClaimTransactionLockAsync(masterId, dependentId, token);
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ITransactionLockDependent<TModel, TId, TDependentId>>(Service);
            return implementation.ClaimTransactionLockAndReadAsync(masterId, dependentId, token);
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

        /// <inheritdoc />
        public Task<TId> GetDependentUniqueIdAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IGetDependentUniqueId<TId, TDependentId>>(Service);
            return implementation.GetDependentUniqueIdAsync(masterId, dependentId, token);
        }
    }
}
