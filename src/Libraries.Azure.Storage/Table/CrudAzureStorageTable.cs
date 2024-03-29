﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Table
{
    /// <summary>
    /// General class for storing any <see cref="TItem"/> in memory.
    /// </summary>
    public class CrudAzureStorageTable<TItem, TId> : 
        CrudAzureStorageTable<TItem, TItem, TId>, 
        ICrud<TItem, TId>
        where TItem : IOptimisticConcurrencyControlByETag
    {
        public CrudAzureStorageTable(string connectionString, string name)
        : base(connectionString, name)
        {
        }
    }

    /// <summary>
    /// General class for storing any <see cref="TModel"/> in memory.
    /// </summary>
    public class CrudAzureStorageTable<TItemCreate, TModel, TId> : 
        ICrud<TItemCreate, TModel, TId> 
        where TModel : TItemCreate, IOptimisticConcurrencyControlByETag
    {
        private readonly CrudConvenience<TItemCreate, TModel, TId> _convenience;
        protected AzureStorageTable<TItemCreate, TModel> Table { get; }

        public CrudAzureStorageTable(string connectionString, string name)
        {
            Table = new AzureStorageTable<TItemCreate, TModel>(connectionString, name);
            _convenience = new CrudConvenience<TItemCreate, TModel,TId>(this);
        }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TItemCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var id = StorageHelper.CreateNewId<TId>();
            await CreateWithSpecifiedIdAsync(id, item, token);
            return id;
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TItemCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var id = await CreateAsync(item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(TId id, TItemCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var key = id.ToString();
            item.TrySetPrimaryKey(id);
            await Table.CreateAsync(key, key, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TItemCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await CreateWithSpecifiedIdAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = default)
        {
            throw new FulcrumNotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId id, TId itemId, CancellationToken token = default)
        {
            throw new FulcrumNotImplementedException();
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));

            var key = id.ToString();
            return await Table.ReadAsync(key, key, token);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(TId id, TModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var key = id.ToString();
            await Table.UpdateAsync(key, key, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await UpdateAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Idempotent, i.e. will not throw an exception if the item does not exist.
        /// </remarks>
        public async Task DeleteAsync(TId id, CancellationToken token = default)
        {
            var key = id.ToString();
            await Table.DeleteAsync(key, key, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset = 0, int? limit = null, CancellationToken token = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            limit = limit ?? int.MaxValue;
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            return await Table.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TModel>> ReadAllAsync(int limit = 2147483647, CancellationToken token = default)
        {
            return StorageHelper.ReadPagesAsync<TModel>((offset, ct) => ReadAllWithPagingAsync(offset, null, ct), limit,
                token);
        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken token = default)
        {
            await Table.DeleteItemsAsync(token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, TimeSpan? lockTimeSpan = null, TId currentLockId = default,
            CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken token = default)
        {
            return _convenience.ClaimTransactionLockAndReadAsync(id, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            return _convenience.SearchAsync(details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }
    }
}
