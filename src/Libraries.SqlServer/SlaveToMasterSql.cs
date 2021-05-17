﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
        public Task<Guid> CreateAsync(Guid masterId, TSlaveModelCreate item, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> CreateAndReturnAsync(Guid masterId, TSlaveModelCreate item, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(Guid masterId, Guid slaveId, TSlaveModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateWithSpecifiedIdAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> CreateWithSpecifiedIdAndReturnAsync(Guid masterId, Guid slaveId, TSlaveModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.CreateWithSpecifiedIdAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> ReadAsync(Guid masterId, Guid slaveId, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.ReadAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> ReadAsync(SlaveToMasterId<Guid> id, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotNull(id, nameof(id));
            InternalContract.RequireValidated(id, nameof(id));
            return SlaveTableHandler.ReadAsync(id.SlaveId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TSlaveModel>> ReadChildrenWithPagingAsync(Guid parentId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TSlaveModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            return StorageHelper.ReadPagesAsync((offset, t) => ReadChildrenWithPagingAsync(parentId, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TSlaveModel>> SearchChildrenAsync(Guid parentId, SearchDetails<TSlaveModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            
            var param = details.WhereAsModel == null ? new TSlaveModel() : details.WhereAsModel;
            var property = typeof(TSlaveModel).GetProperty(ParentColumnName);
            FulcrumAssert.IsNotNull(property, CodeLocation.AsString());
            property?.SetValue(param, parentId);
            var whereList = SearchHelper.WhereAsList(details);
            whereList.Add($"{ParentColumnName} = @{ParentColumnName}");
            var where = string.Join(" AND ", whereList);

            var orderList = SearchHelper.OrderByAsList(details);
            string orderBy = null;
            if (orderList.Any())
            {
                orderBy = string.Join(", ", orderList);
            }

            return SearchWhereAsync(where, orderBy, param, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> SearchFirstChildAsync(Guid parentId, SearchDetails<TSlaveModel> details,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _convenience.SearchFirstChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> FindUniqueChildAsync(Guid parentId, SearchDetails<TSlaveModel> details,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(Guid masterId, Guid slaveId, TSlaveModel item, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.UpdateAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task<TSlaveModel> UpdateAndReturnAsync(Guid masterId, Guid slaveId, TSlaveModel item,
            CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.UpdateAndReturnAsync(slaveId, item, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(Guid masterId, Guid slaveId, CancellationToken token = new CancellationToken())
        {
            return SlaveTableHandler.DeleteAsync(slaveId, token);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(Guid parentId, CancellationToken token = default(CancellationToken))
        {
            return DeleteWhereAsync($"[{ParentColumnName}] = @ParentId", new { ParentId = parentId }, token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<Guid>> ClaimLockAsync(Guid masterId, Guid slaveId, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(Guid masterId, Guid slaveId, Guid lockId, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<SlaveLock<Guid>> ClaimDistributedLockAsync(Guid masterId, Guid slaveId, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(Guid masterId, Guid slaveId, Guid lockId,
            CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task ClaimTransactionLockAsync(Guid masterId, Guid slaveId, CancellationToken token = default(CancellationToken))
        {
            var selectStatement =
                $"SELECT {SqlHelper.ReadColumnNames(TableMetadata)} FROM [{TableMetadata.TableName}] WITH (ROWLOCK, UPDLOCK, READPAST) WHERE Id=@Id";
            var result = await SearchAdvancedSingleAsync(selectStatement, new { Id = slaveId }, token);
            if (result == null)
            {
                throw new FulcrumTryAgainException(
                    $"Item {slaveId} in table {TableMetadata.TableName} was already locked by another client.")
                {
                    RecommendedWaitTimeInSeconds = 1
                };
            }
        }
    }
}
