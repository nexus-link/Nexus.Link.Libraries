﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http;
#endif
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.AspNet.Controllers;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.AspNet.ControllerHelpers
{
    /// <inheritdoc cref="CrudControllerHelper{TModel, TModel}" />
    public class CrudControllerHelper<TModel> :
        CrudControllerHelper<TModel, TModel>,
        ICrud<TModel, string>
    {
        /// <inheritdoc />
        public CrudControllerHelper(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <inheritdoc cref="CrudControllerBase" />
    public class CrudControllerHelper<TModelCreate, TModel> :
        ICrud<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        /// <summary>
        /// The logic to be used
        /// </summary>
        protected readonly ICrud<TModelCreate, TModel, string> Logic;

        private readonly CrudConvenience<TModelCreate, TModel, string> _convenience;

        /// <inheritdoc />
        public CrudControllerHelper(ICrudable<TModel, string> logic)
        {
            Logic = new CrudPassThrough<TModelCreate, TModel, string>(logic);
            _convenience = new CrudConvenience<TModelCreate, TModel, string>(this);
        }

        /// <inheritdoc />
        public virtual async Task<string> CreateAsync(TModelCreate item, CancellationToken token = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            var id = await Logic.CreateAsync(item, token);
            FulcrumAssert.IsNotNullOrWhiteSpace(id);
            return id;
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default)
        {
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            var createdItem = await Logic.CreateAndReturnAsync(item, token);
            FulcrumAssert.IsNotNull(createdItem);
            FulcrumAssert.IsValidated(item, nameof(item));
            return createdItem;
        }

        /// <inheritdoc />
        public virtual async Task CreateWithSpecifiedIdAsync(string id, TModelCreate item, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            await Logic.CreateWithSpecifiedIdAsync(id, item, token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string id, TModelCreate item,
            CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            var createdItem = await Logic.CreateWithSpecifiedIdAndReturnAsync(id, item, token);
            FulcrumAssert.IsNotNull(createdItem);
            FulcrumAssert.IsValidated(createdItem);
            return createdItem;
        }

        /// <inheritdoc />
        public virtual async Task<TModel> ReadAsync(string id, CancellationToken token = default)
        {
            ServiceContract.RequireNotDefaultValue(id, nameof(id));
            var item = await Logic.ReadAsync(id, token);
            FulcrumAssert.IsValidated(item);
            return item;
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default)
        {
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                ServiceContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }

            var page = await Logic.ReadAllWithPagingAsync(offset, limit, token);
            FulcrumAssert.IsNotNull(page?.Data);
            FulcrumAssert.IsValidated(page?.Data);
            return page;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken token = default)
        {
            ServiceContract.RequireGreaterThan(0, limit, nameof(limit));
            var items = await Logic.ReadAllAsync(limit, token);
            FulcrumAssert.IsNotNull(items);
            FulcrumAssert.IsValidated(items);
            return items;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> SearchAsync([FromBody] SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(details, nameof(details));
            ServiceContract.RequireValidated(details, nameof(details));
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                ServiceContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }

            var page = await Logic.SearchAsync(details, offset, limit, cancellationToken);
            FulcrumAssert.IsNotNull(page?.Data);
            FulcrumAssert.IsValidated(page?.Data);
            return page;
        }

        /// <inheritdoc />
        public Task<TModel> SearchFirstAsync([FromBody] SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync([FromBody] SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(string id, TModel item, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            await Logic.UpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> UpdateAndReturnAsync(string id, TModel item, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNull(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            var updatedItem = await Logic.UpdateAndReturnAsync(id, item, token);
            FulcrumAssert.IsNotNull(updatedItem);
            FulcrumAssert.IsValidated(updatedItem);
            return updatedItem;
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(string id, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            await Logic.DeleteAsync(id, token);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAllAsync(CancellationToken token = default)
        {
            await Logic.DeleteAllAsync(token);
        }

        /// <inheritdoc />
        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public virtual async Task<Lock<string>> ClaimLockAsync(string id, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            var @lock = await Logic.ClaimLockAsync(id, token);
            FulcrumAssert.IsNotNull(@lock);
            FulcrumAssert.IsValidated(@lock);
            return @lock;
        }

        /// <inheritdoc />
        [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
        public virtual async Task ReleaseLockAsync(string id, string lockId, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            await Logic.ReleaseLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public async Task<Lock<string>> ClaimDistributedLockAsync(string id, TimeSpan? lockTimeSpan = null,
            string currentLockId = default,
            CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            var @lock = await Logic.ClaimDistributedLockAsync(id, lockTimeSpan, currentLockId, token);
            FulcrumAssert.IsNotNull(@lock);
            FulcrumAssert.IsValidated(@lock);
            return @lock;
        }

        /// <inheritdoc />
        public async Task ReleaseDistributedLockAsync(string id, string lockId, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(id, nameof(id));
            ServiceContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            await Logic.ReleaseDistributedLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(string id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(string id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}