using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.PassThrough
{
    /// <inheritdoc cref="CrudPassThrough{TModelCreate,TModel,TId}" />
    public class CrudPassThrough<TModel, TId> : CrudPassThrough<TModel, TModel, TId>, ICrud<TModel, TId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public CrudPassThrough(ICrudable<TModel, TId> service)
        :base(service)
        {
        }
    }

    /// <inheritdoc cref="ICrud{TModel,TId}" />
    public class CrudPassThrough<TModelCreate, TModel, TId> : ICrud<TModelCreate, TModel, TId> where TModel : TModelCreate
    {
        /// <summary>
        /// The service to pass the calls to.
        /// </summary>
        protected readonly ICrudable<TModel, TId> Service;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service">The crud class to pass things down to.</param>
        public CrudPassThrough(ICrudable<TModel, TId> service)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            Service = service;
        }

        /// <inheritdoc />
        public virtual Task<TId> CreateAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreate<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateAndReturn<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public virtual Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateWithSpecifiedId<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateWithSpecifiedIdAsync(id, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item,
            CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateWithSpecifiedId<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateWithSpecifiedIdAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IRead<TModel, TId>>(Service);
            return implementation.ReadAsync(id, token);
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadAllWithPaging<TModel, TId>>(Service);
            return implementation.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadAll<TModel, TId>>(Service);
            return implementation.ReadAllAsync(limit, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchAsync(object condition, object order, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ISearch<TModel, TId>>(Service);
            return implementation.SearchAsync(condition, order, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdate<TModel, TId>>(Service);
            return implementation.UpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        public virtual Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdateAndReturn<TModel, TId>>(Service);
            return implementation.UpdateAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDelete<TId>>(Service);
            return implementation.DeleteAsync(id, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDeleteAll>(Service);
            return implementation.DeleteAllAsync(token);
        }

        /// <inheritdoc />
        public virtual Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
#pragma warning disable 618
            var implementation = CrudHelper.GetImplementationOrThrow<ILockable<TId>>(Service);
#pragma warning restore 618
            return implementation.ClaimLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId id, TId lockId, CancellationToken token = default(CancellationToken))
        {
#pragma warning disable 618
            var implementation = CrudHelper.GetImplementationOrThrow<ILockable<TId>>(Service);
#pragma warning restore 618
            return implementation.ReleaseLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDistributedLock<TId>>(Service);
            return implementation.ClaimDistributedLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDistributedLock<TId>>(Service);
            return implementation.ReleaseDistributedLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ITransactionLock<TId>>(Service);
            return implementation.ClaimTransactionLockAsync(id, token);
        }
    }
}
