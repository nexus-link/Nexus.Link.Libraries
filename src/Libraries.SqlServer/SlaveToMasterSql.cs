﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer
{
    /// <inheritdoc cref="SlaveToMasterSql{TSlaveModelCreate, TSlaveModel,TMasterModel}" />
    [Obsolete("Use DependentToMasterSql. Obsolete since 2021-08-27.")]
    public class SlaveToMasterSql<TSlaveModel, TMasterModel> :
        SlaveToMasterSql<TSlaveModel, TSlaveModel, TMasterModel>,
        ICrudSlaveToMaster<TSlaveModel, Guid>
        where TSlaveModel : IUniquelyIdentifiable<Guid>, new()
        where TMasterModel : IUniquelyIdentifiable<Guid>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="masterTableHandler"></param>
        public SlaveToMasterSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName,
            CrudSql<TSlaveModel> slaveTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(connectionString, tableMetadata, parentColumnName, slaveTableHandler, masterTableHandler)
        {
        }
    }

    /// <inheritdoc cref="ICrudSlaveToMaster{TModelCreate, TModel,TId}" />
    [Obsolete("Use DependentToMasterSql. Obsolete since 2021-08-27.")]
    public class SlaveToMasterSql<TSlaveModelCreate, TSlaveModel, TMasterModel> :
        CrudSql<TSlaveModelCreate, TSlaveModel>,
        ICrudSlaveToMaster<TSlaveModelCreate, TSlaveModel, Guid>
            where TSlaveModel : TSlaveModelCreate, IUniquelyIdentifiable<Guid>, new()
            where TMasterModel : IUniquelyIdentifiable<Guid>
    {
        private readonly SlaveToMasterConvenience<TSlaveModelCreate, TSlaveModel, Guid> _convenience;

        public string ParentColumnName { get; }
        protected CrudSql<TSlaveModelCreate, TSlaveModel> SlaveTableHandler { get; }
        protected CrudSql<TMasterModel> MasterTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SlaveToMasterSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName, CrudSql<TSlaveModelCreate, TSlaveModel> slaveTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            SlaveTableHandler = slaveTableHandler;
            MasterTableHandler = masterTableHandler;
            _convenience = new SlaveToMasterConvenience<TSlaveModelCreate, TSlaveModel, Guid>(this);
        }

        /// <inheritdoc />
        public Task<Guid> CreateAsync(Guid masterId, TSlaveModelCreate item, CancellationToken token = default)
        {
            return SlaveTableHandler.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> CreateAndReturnAsync(Guid masterId, TSlaveModelCreate item, CancellationToken token = default)
        {
            return SlaveTableHandler.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(Guid masterId, Guid slaveId, TSlaveModelCreate item,
            CancellationToken token = default)
        {
            return SlaveTableHandler.CreateWithSpecifiedIdAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> CreateWithSpecifiedIdAndReturnAsync(Guid masterId, Guid slaveId, TSlaveModelCreate item,
            CancellationToken token = default)
        {
            return SlaveTableHandler.CreateWithSpecifiedIdAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> ReadAsync(Guid masterId, Guid slaveId, CancellationToken token = default)
        {
            return SlaveTableHandler.ReadAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> ReadAsync(SlaveToMasterId<Guid> id, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(id, nameof(id));
            InternalContract.RequireValidated(id, nameof(id));
            return SlaveTableHandler.ReadAsync(id.SlaveId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TSlaveModel>> ReadChildrenWithPagingAsync(Guid parentId, int offset, int? limit = null, CancellationToken token = default)
        {
            return SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TSlaveModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            return StorageHelper.ReadPagesAsync((offset, t) => ReadChildrenWithPagingAsync(parentId, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TSlaveModel>> SearchChildrenAsync(Guid parentId, SearchDetails<TSlaveModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            var whereAsModel = details.GetWhereAsModel("%", "_");
            var param = whereAsModel == null ? new TSlaveModel() : whereAsModel;
            var property = typeof(TSlaveModel).GetProperty(ParentColumnName);
            FulcrumAssert.IsNotNull(property, CodeLocation.AsString());
            property?.SetValue(param, parentId);

            var where = CrudSearchHelper.GetWhereStatement(details, ParentColumnName);
            var orderBy = CrudSearchHelper.GetOrderByStatement(details);

            return SearchWhereAsync(where, orderBy, param, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> FindUniqueChildAsync(Guid parentId, SearchDetails<TSlaveModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(Guid masterId, Guid slaveId, TSlaveModel item, CancellationToken token = default)
        {
            return SlaveTableHandler.UpdateAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> UpdateAndReturnAsync(Guid masterId, Guid slaveId, TSlaveModel item,
            CancellationToken token = default)
        {
            return SlaveTableHandler.UpdateAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(Guid masterId, Guid slaveId, CancellationToken token = default)
        {
            return SlaveTableHandler.DeleteAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(Guid parentId, CancellationToken token = default)
        {
            return DeleteWhereAsync($"[{ParentColumnName}] = @ParentId", new { ParentId = parentId }, token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<Guid>> ClaimLockAsync(Guid masterId, Guid slaveId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(Guid masterId, Guid slaveId, Guid lockId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<SlaveLock<Guid>> ClaimDistributedLockAsync(Guid masterId, Guid slaveId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(Guid masterId, Guid slaveId, Guid lockId,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task ClaimTransactionLockAsync(Guid masterId, Guid slaveId, CancellationToken token = default)
        {
            var result = await SearchSingleAndLockWhereAsync("Id=@Id", new { Id = slaveId }, token);
            if (result == null)
            {
                throw new FulcrumResourceLockedException(
                    $"Item {slaveId} in table {TableMetadata.TableName} was already locked by another client.")
                {
                    RecommendedWaitTimeInSeconds = 1
                };
            }
        }

        /// <inheritdoc />
        public Task<TSlaveModel> ClaimTransactionLockAndReadAsync(Guid masterId, Guid slaveId,
            CancellationToken token = default)
        {
            return _convenience.ClaimTransactionLockAndReadAsync(masterId, slaveId, token);
        }
    }
}
