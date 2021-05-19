using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Azure.Storage.Table
{
    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class
        SlaveToMasterAzureStorageTable<TItem, TId> :
            SlaveToMasterAzureStorageTable<TItem, TItem, TId>,
            ICrudSlaveToMaster<TItem, TId>
        where TItem : IOptimisticConcurrencyControlByETag
    {
        public SlaveToMasterAzureStorageTable(string connectionString, string name)
        : base(connectionString, name)
        {
        }
    }

    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class SlaveToMasterAzureStorageTable<TItemCreate, TItem, TId> :
        ICrudSlaveToMaster<TItemCreate, TItem, TId>
        where TItem : TItemCreate, IOptimisticConcurrencyControlByETag
    {
        private SlaveToMasterConvenience<TItemCreate, TItem, TId> _convenience;
        protected AzureStorageTable<TItemCreate, TItem> Table { get; }

        public SlaveToMasterAzureStorageTable(string connectionString, string name)
        {
            Table = new AzureStorageTable<TItemCreate, TItem>(connectionString, name);
            _convenience = new SlaveToMasterConvenience<TItemCreate, TItem, TId>(this);
        }

        /// <inheritdoc />
        public async Task<TId> CreateAsync(TId masterId, TItemCreate item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));

            var slaveId = StorageHelper.CreateNewId<TId>();
            await CreateWithSpecifiedIdAsync(masterId, slaveId, item, token);
            return slaveId;
        }

        /// <inheritdoc />
        public async Task<TItem> CreateAndReturnAsync(TId masterId, TItemCreate item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            var slaveId = await CreateAsync(masterId, item, token);
            return await ReadAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(TId masterId, TId slaveId, TItemCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            var serverMasterId = MapperHelper.MapToType<string, TId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<string, TId>(slaveId);

            return Table.CreateAsync(serverMasterId, serverSlaveId, item, token);
        }

        /// <inheritdoc />
        public async Task<TItem> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TId slaveId, TItemCreate item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            await CreateWithSpecifiedIdAsync(masterId, slaveId, item, token);
            return await ReadAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TItem>> ReadChildrenWithPagingAsync(TId masterId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            limit = limit ?? int.MaxValue;
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            return await Table.ReadAllWithPagingAsync(masterId.ToString(), offset, limit, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TItem>> ReadChildrenAsync(TId parentId, int limit = 2147483647, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return StorageHelper.ReadPagesAsync<TItem>((offset, ct) => ReadChildrenWithPagingAsync(parentId, offset, null, ct), limit, token);
        }

        /// <inheritdoc />
        public async Task<TItem> ReadAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var serverMasterId = MapperHelper.MapToType<string, TId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<string, TId>(slaveId);
            return await Table.ReadAsync(serverMasterId, serverSlaveId, token);
        }

        /// <inheritdoc />
        public Task<TItem> ReadAsync(SlaveToMasterId<TId> id, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotNull(id, nameof(id));
            InternalContract.RequireValidated(id, nameof(id));
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(TId masterId, TId slaveId, TItem item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var serverMasterId = MapperHelper.MapToType<string, TId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<string, TId>(slaveId);
            await Table.UpdateAsync(serverMasterId, serverSlaveId, item, token);
        }

        /// <inheritdoc />
        public async Task<TItem> UpdateAndReturnAsync(TId masterId, TId slaveId, TItem item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            await UpdateAsync(masterId, slaveId, item, token);
            return await ReadAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var serverMasterId = MapperHelper.MapToType<string, TId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<string, TId>(slaveId);

            await Table.DeleteAsync(serverMasterId, serverSlaveId, token);
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(TId masterId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            var serverMasterId = MapperHelper.MapToType<string, TId>(masterId);
            await Table.DeleteItemsAsync(serverMasterId, token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<TId>> ClaimLockAsync(TId masterId, TId slaveId, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId masterId, TId slaveId, TId lockId, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<SlaveLock<TId>> ClaimDistributedLockAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId masterId, TId slaveId, TId lockId,
            CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TItem> ClaimTransactionLockAndReadAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken))
        {
            return _convenience.ClaimTransactionLockAndReadAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TItem>> SearchChildrenAsync(TId parentId, SearchDetails<TItem> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _convenience.SearchChildrenAsync(parentId, details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TItem> FindUniqueChildAsync(TId parentId, SearchDetails<TItem> details,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }
    }
}