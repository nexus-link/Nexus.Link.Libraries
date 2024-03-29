﻿using System;
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
        public virtual Task<TId> CreateAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreate<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateAsync(item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateAndReturn<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateAndReturnAsync(item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateWithSpecifiedId<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateWithSpecifiedIdAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ICreateWithSpecifiedId<TModelCreate, TModel, TId>>(Service);
            return implementation.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IRead<TModel, TId>>(Service);
            return implementation.ReadAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadAllWithPaging<TModel, TId>>(Service);
            return implementation.ReadAllWithPagingAsync(offset, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IReadAll<TModel, TId>>(Service);
            return implementation.ReadAllAsync(limit, cancellationToken );
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ISearch<TModel, TId>>(Service);
            return implementation.SearchAsync(details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ISearch<TModel, TId>>(Service);
            return implementation.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdate<TModel, TId>>(Service);
            return implementation.UpdateAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IUpdateAndReturn<TModel, TId>>(Service);
            return implementation.UpdateAndReturnAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TId id, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDelete<TId>>(Service);
            return implementation.DeleteAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task DeleteAllAsync(CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDeleteAll>(Service);
            return implementation.DeleteAllAsync(cancellationToken );
        }

        /// <inheritdoc />
        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public virtual Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken cancellationToken  = default)
        {
#pragma warning disable 618
            var implementation = CrudHelper.GetImplementationOrThrow<ILockable<TId>>(Service);
#pragma warning restore 618
            return implementation.ClaimLockAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public Task ReleaseLockAsync(TId id, TId lockId, CancellationToken cancellationToken  = default)
        {
#pragma warning disable 618
            var implementation = CrudHelper.GetImplementationOrThrow<ILockable<TId>>(Service);
#pragma warning restore 618
            return implementation.ReleaseLockAsync(id, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, TimeSpan? lockTimeSpan = null, TId currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDistributedLock<TId>>(Service);
            return implementation.ClaimDistributedLockAsync(id, lockTimeSpan, currentLockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<IDistributedLock<TId>>(Service);
            return implementation.ReleaseDistributedLockAsync(id, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ITransactionLock<TModel, TId>>(Service);
            return implementation.ClaimTransactionLockAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken cancellationToken  = default)
        {
            var implementation = CrudHelper.GetImplementationOrThrow<ITransactionLock<TModel, TId>>(Service);
            return implementation.ClaimTransactionLockAndReadAsync(id, cancellationToken );
        }
    }
}
