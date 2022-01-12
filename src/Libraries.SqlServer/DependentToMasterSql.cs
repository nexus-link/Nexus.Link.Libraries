using System;
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
    /// <inheritdoc cref="DependentToMasterSql{TDependentModelCreate, TDependentModel,TMasterModel}" />
    public class DependentToMasterSql<TDependentModel, TMasterModel, TDependentId> :
        DependentToMasterSql<TDependentModel, TDependentModel, TMasterModel, TDependentId>,
        ICrudDependentToMaster<TDependentModel, Guid, TDependentId>
        where TDependentModel : IUniquelyIdentifiableDependentWithUniqueId<Guid, TDependentId>, new()
        where TMasterModel : IUniquelyIdentifiable<Guid>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="dependentTableHandler"></param>
        /// <param name="masterTableHandler"></param>
        [Obsolete("Use DependentToMasterSql(IDatabaseOptions, ISqlTableMetadata, ...) instead. Obsolete since 2021-01-07.", error: false)]
        public DependentToMasterSql(string connectionString, ISqlTableMetadata tableMetadata,
            CrudSql<TDependentModel> dependentTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(connectionString, tableMetadata, dependentTableHandler, masterTableHandler)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="dependentTableHandler"></param>
        /// <param name="masterTableHandler"></param>
        [Obsolete("Use DependentToMasterSql(IDatabaseOptions, ISqlTableMetadata, ...) instead. Obsolete since 2021-01-07.", error: false)]
        public DependentToMasterSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata,
            CrudSql<TDependentModel> dependentTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(options, tableMetadata, dependentTableHandler, masterTableHandler)
        {
        }
    }

    /// <inheritdoc cref="ICrudDependentToMaster{TModelCreate, TModel,TId}" />
    public class DependentToMasterSql<TDependentModelCreate, TDependentModel, TMasterModel, TDependentId> :
        TableBase<TDependentModel>,
        ICrudDependentToMaster<TDependentModelCreate, TDependentModel, Guid, TDependentId>
        where TDependentModelCreate : IUniquelyIdentifiableDependent<Guid, TDependentId>
        where TDependentModel : TDependentModelCreate, IUniquelyIdentifiableDependentWithUniqueId<Guid, TDependentId>, new() 
        where TMasterModel : IUniquelyIdentifiable<Guid>
    {
        private readonly DependentToMasterConvenience<TDependentModelCreate, TDependentModel, Guid, TDependentId> _convenience;

        protected CrudSql<TDependentModelCreate, TDependentModel> DependentTableHandler { get; }
        protected CrudSql<TMasterModel> MasterTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        [Obsolete("Use DependentToMasterSql(IDatabaseOptions, ISqlTableMetadata, ...) instead. Obsolete since 2021-01-07.", error: false)]
        public DependentToMasterSql(string connectionString, ISqlTableMetadata tableMetadata, CrudSql<TDependentModelCreate, TDependentModel> dependentTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = nameof(IUniquelyIdentifiableDependent<Guid, TDependentId>.MasterId);
            DependentTableHandler = dependentTableHandler;
            MasterTableHandler = masterTableHandler;
            _convenience = new DependentToMasterConvenience<TDependentModelCreate, TDependentModel, Guid, TDependentId>(this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DependentToMasterSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata, CrudSql<TDependentModelCreate, TDependentModel> dependentTableHandler, CrudSql<TMasterModel> masterTableHandler)
            : base(options, tableMetadata)
        {
            ParentColumnName = nameof(IUniquelyIdentifiableDependent<Guid, TDependentId>.MasterId);
            DependentTableHandler = dependentTableHandler;
            MasterTableHandler = masterTableHandler;
            _convenience = new DependentToMasterConvenience<TDependentModelCreate, TDependentModel, Guid, TDependentId>(this);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(Guid masterId, TDependentId dependentId, TDependentModelCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireAreEqual(item.MasterId, masterId, nameof(masterId));
            InternalContract.RequireAreEqual(item.DependentId, dependentId, nameof(dependentId));
            await DependentTableHandler.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TDependentModel> CreateWithSpecifiedIdAndReturnAsync(Guid masterId, TDependentId dependentId, TDependentModelCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireAreEqual(item.MasterId, masterId, nameof(masterId));
            InternalContract.RequireAreEqual(item.DependentId, dependentId, nameof(dependentId));
            return DependentTableHandler.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TDependentModel> ReadAsync(Guid masterId, TDependentId dependentId, CancellationToken token = default)
        {
            return DependentTableHandler.FindUniqueAsync(
                new SearchDetails<TDependentModel>(new {MasterId = masterId, DependentId = dependentId}), token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TDependentModel>> ReadChildrenWithPagingAsync(Guid masterId, int offset, int? limit = null, CancellationToken token = default)
        {
            return SearchWhereAsync($"[MasterId] = @MasterId", null, new { MasterId = masterId }, offset, limit, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TDependentModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            return StorageHelper.ReadPagesAsync((offset, t) => ReadChildrenWithPagingAsync(parentId, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TDependentModel>> SearchChildrenAsync(Guid masterId, SearchDetails<TDependentModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            var whereAsModel = details.GetWhereAsModel("%", "_");
            var param = whereAsModel == null ? new TDependentModel() : whereAsModel;
            var property = typeof(TDependentModel).GetProperty(ParentColumnName);
            FulcrumAssert.IsNotNull(property, CodeLocation.AsString());
            property?.SetValue(param, masterId);

            var where = CrudSearchHelper.GetWhereStatement(details, ParentColumnName);
            var orderBy = CrudSearchHelper.GetOrderByStatement(details);

            return SearchWhereAsync(where, orderBy, param, offset, limit, cancellationToken);
        }

        public string ParentColumnName { get; set; }

        /// <inheritdoc />
        public Task<TDependentModel> FindUniqueChildAsync(Guid parentId, SearchDetails<TDependentModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Guid masterId, TDependentId dependentId, TDependentModel item, CancellationToken token = default)
        {
            var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, token);
            await DependentTableHandler.UpdateAsync(uniqueId, item, token);
        }

        /// <inheritdoc />
        public async Task<TDependentModel> UpdateAndReturnAsync(Guid masterId, TDependentId dependentId, TDependentModel item,
            CancellationToken token = default)
        {
            var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, token);
            return await DependentTableHandler.UpdateAndReturnAsync(uniqueId, item, token);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid masterId, TDependentId dependentId, CancellationToken token = default)
        {
            try
            {
                var uniqueId = await GetDependentUniqueIdAsync(masterId, dependentId, token);
                await DependentTableHandler.DeleteAsync(uniqueId, token);
            }
            catch (FulcrumNotFoundException)
            {
                // This is OK, since we intend to delete it.
            }
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(Guid masterId, CancellationToken token = default)
        {
            return DeleteWhereAsync($"[{ParentColumnName}] = @MasterId", new { MasterId = masterId }, token);
        }

        /// <inheritdoc />
        public Task<DependentLock<Guid, TDependentId>> ClaimDistributedLockAsync(Guid masterId,
            TDependentId dependentId, Guid currentLockId = default, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(Guid masterId, TDependentId dependentId, Guid lockId,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task ClaimTransactionLockAsync(Guid masterId, TDependentId dependentId, CancellationToken token = default)
        {
            await ClaimTransactionLockAndReadAsync(masterId, dependentId, token);
        }

        /// <inheritdoc />
        public async Task<TDependentModel> ClaimTransactionLockAndReadAsync(Guid masterId, TDependentId dependentId,
            CancellationToken token = default)
        {
            var result = await SearchSingleAndLockWhereAsync("MasterId=@MasterId AND DependentId=@DependentId", new { MasterId = masterId, DependentId = dependentId }, token);
            if (result == null)
            {
                throw new FulcrumTryAgainException(
                    $"Item {dependentId} in table {TableMetadata.TableName} was already locked by another client.")
                {
                    RecommendedWaitTimeInSeconds = 1
                };
            }
            return result;
        }

        /// <inheritdoc />
        public Task<Guid> GetDependentUniqueIdAsync(Guid masterId, TDependentId dependentId, CancellationToken token = default)
        {
            return _convenience.GetDependentUniqueIdAsync(masterId, dependentId, token);
        }
    }
}
