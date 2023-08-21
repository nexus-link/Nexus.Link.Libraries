using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
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
    public class ManyToOneSql<TManyModel, TOneModel> :
        CrudSql<TManyModel>,
        ICrudManyToOne<TManyModel, Guid>
        where TManyModel : IUniquelyIdentifiable<Guid>, new()
        where TOneModel : IUniquelyIdentifiable<Guid>
    {
        private readonly ManyToOneConvenience<TManyModel, TManyModel, Guid> _convenience;

        public string ParentColumnName { get; }
        protected TableBase<TOneModel> ParentTable { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="parentTable"></param>
        [Obsolete("Use ManyToOneSql(IDatabaseOptions, ISqlTableMetadata, ...) instead. Obsolete since 2021-01-07.", error: false)]
        public ManyToOneSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName,
            TableBase<TOneModel> parentTable)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            ParentTable = parentTable;
            _convenience = new ManyToOneConvenience<TManyModel, TManyModel, Guid>(this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options for this table</param>
        /// <param name="tableMetadata">The configuration for this table</param>
        /// <param name="parentColumnName">The name of the column that points out an id in the parent table.</param>
        /// <param name="parentTable">The parent table </param>
        public ManyToOneSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata, string parentColumnName,
            TableBase<TOneModel> parentTable)
            : base(options, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            ParentTable = parentTable;
            _convenience = new ManyToOneConvenience<TManyModel, TManyModel, Guid>(this);
        }

        /// <summary>
        /// Read all referenced items that a specific column references.
        /// </summary>
        /// <param name="groupColumnName"></param>
        /// <param name="groupColumnValue"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        /// <remarks>This method is here to support the <see cref="ManyToManySql{TManyToManyModel,TReferenceModel1,TReferenceModel2}."/></remarks>
        internal async Task<PageEnvelope<TOneModel>> ReadAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, int offset, int? limit = null, CancellationToken token = default)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{ParentTable.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await ParentTable.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.GetOrderBy("many"), new { ColumnValue = groupColumnValue }, offset, limit, token);
        }

        /// <summary>
        /// Delete all referenced items that a specific column references.
        /// </summary>
        /// <param name="groupColumnName"></param>
        /// <param name="groupColumnValue"></param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        /// <remarks>This method is here to support the <see cref="ManyToManySql{TManyToManyModel,TReferenceModel1,TReferenceModel2}."/></remarks>
        internal async Task DeleteAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, CancellationToken token)
        {
            var deleteStatement = "DELETE one" +
                             $" FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{ParentTable.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            await ParentTable.ExecuteAsync(deleteStatement, new { ColumnValue = groupColumnValue }, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TManyModel>> ReadChildrenWithPagingAsync(Guid parentId, int offset, int? limit = null, CancellationToken token = default)
        {
            return await SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TManyModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadChildrenWithPagingAsync(parentId, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(Guid parentId, CancellationToken token = default)
        {
            await DeleteWhereAsync($"[{ParentColumnName}] = @ParentId", new { ParentId = parentId }, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> SearchChildrenAsync(Guid parentId, SearchDetails<TManyModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            
            var whereAsModel = details.GetWhereAsModel("%", "_");
            var param = whereAsModel == null ? new TManyModel() : whereAsModel;
            var property = typeof(TManyModel).GetProperty(ParentColumnName);
            FulcrumAssert.IsNotNull(property, CodeLocation.AsString());
            property?.SetValue(param, parentId);
            
            var where = CrudSearchHelper.GetWhereStatement(details, ParentColumnName);
            var orderBy = CrudSearchHelper.GetOrderByStatement(details);
            return SearchWhereAsync(where, orderBy, param, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TManyModel> FindUniqueChildAsync(Guid parentId, SearchDetails<TManyModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Guid> CreateChildAsync(Guid parentId, TManyModel item, CancellationToken token = default)
        {
            return _convenience.CreateChildAsync(parentId, item, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateChildAndReturnAsync(Guid parentId, TManyModel item, CancellationToken token = default)
        {
            return _convenience.CreateChildAndReturnAsync(parentId, item, token);
        }

        /// <inheritdoc />
        public Task CreateChildWithSpecifiedIdAsync(Guid parentId, Guid childId, TManyModel item, CancellationToken token = default)
        {
            return _convenience.CreateChildWithSpecifiedIdAsync(parentId, childId, item, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateChildWithSpecifiedIdAndReturnAsync(Guid parentId, Guid childId, TManyModel item,
            CancellationToken token = default)
        {
            return _convenience.CreateChildWithSpecifiedIdAndReturnAsync(parentId, childId, item, token);
        }
    }

    public class ManyToOneSql<TManyModelCreate, TManyModel, TOneModel> :
        CrudSql<TManyModelCreate, TManyModel>,
        ICrudManyToOne<TManyModelCreate, TManyModel, Guid>
            where TManyModel : TManyModelCreate, IUniquelyIdentifiable<Guid>, new()
            where TOneModel : IUniquelyIdentifiable<Guid>
    {
        private readonly ManyToOneConvenience<TManyModelCreate, TManyModel, Guid> _convenience;

        public string ParentColumnName { get; }
        protected TableBase<TOneModel> ParentTable { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="parentTable"></param>
        [Obsolete("Use ManyToOneSql(IDatabaseOptions, ISqlTableMetadata, ...) instead. Obsolete since 2021-01-07.", error: false)]
        public ManyToOneSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName, TableBase<TOneModel> parentTable)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            ParentTable = parentTable;
            _convenience = new ManyToOneConvenience<TManyModelCreate, TManyModel, Guid>(this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options for this table</param>
        /// <param name="tableMetadata">The configuration for this table</param>
        /// <param name="parentColumnName">The name of the column that points out an id in the parent table.</param>
        /// <param name="parentTable">The parent table </param>
        public ManyToOneSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata, string parentColumnName, TableBase<TOneModel> parentTable)
            : base(options, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            ParentTable = parentTable;
            _convenience = new ManyToOneConvenience<TManyModelCreate, TManyModel, Guid>(this);
        }

        /// <summary>
        /// Read all referenced items that a specific column references.
        /// </summary>
        /// <param name="groupColumnName"></param>
        /// <param name="groupColumnValue"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        /// <remarks>This method is here to support the <see cref="ManyToManySql{TManyToManyModel,TReferenceModel1,TReferenceModel2}."/></remarks>
        internal async Task<PageEnvelope<TOneModel>> ReadAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, int offset, int? limit = null, CancellationToken token = default)
        {
            var selectRest = $"FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{ParentTable.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await ParentTable.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.GetOrderBy("many"), new { ColumnValue = groupColumnValue }, offset, limit, token);
        }

        /// <summary>
        /// Delete all referenced items that a specific column references.
        /// </summary>
        /// <param name="groupColumnName"></param>
        /// <param name="groupColumnValue"></param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        /// <remarks>This method is here to support the <see cref="ManyToManySql{TManyToManyModel,TReferenceModel1,TReferenceModel2}."/></remarks>
        internal async Task DeleteAllParentsInGroupAsync(string groupColumnName, Guid groupColumnValue, CancellationToken token)
        {
            var deleteStatement = "DELETE one" +
                             $" FROM [{TableMetadata.TableName}] AS many" +
                             $" JOIN [{ParentTable.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            await ParentTable.ExecuteAsync(deleteStatement, new { ColumnValue = groupColumnValue }, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TManyModel>> ReadChildrenWithPagingAsync(Guid parentId, int offset, int? limit = null, CancellationToken token = default)
        {
            return await SearchWhereAsync($"[{ParentColumnName}] = @ParentId", null, new { ParentId = parentId }, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TManyModel>> ReadChildrenAsync(Guid parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadChildrenWithPagingAsync(parentId, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(Guid parentId, CancellationToken token = default)
        {
            await DeleteWhereAsync($"[{ParentColumnName}] = @ParentId", new { ParentId = parentId }, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> SearchChildrenAsync(Guid parentId, SearchDetails<TManyModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            
            var whereAsModel = details.GetWhereAsModel("%", "_");
            var param = whereAsModel == null ? new TManyModel() : whereAsModel;
            var property = typeof(TManyModel).GetProperty(ParentColumnName);
            FulcrumAssert.IsNotNull(property, CodeLocation.AsString());
            property?.SetValue(param, parentId);

            var where = CrudSearchHelper.GetWhereStatement(details, ParentColumnName);
            var orderBy = CrudSearchHelper.GetOrderByStatement(details);

            return SearchWhereAsync(where, orderBy, param, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TManyModel> FindUniqueChildAsync(Guid parentId, SearchDetails<TManyModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task<Guid> CreateChildAsync(Guid parentId, TManyModelCreate item, CancellationToken token = default)
        {
            return _convenience.CreateChildAsync(parentId, item, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateChildAndReturnAsync(Guid parentId, TManyModelCreate item, CancellationToken token = default)
        {
            return _convenience.CreateChildAndReturnAsync(parentId, item, token);
        }

        /// <inheritdoc />
        public Task CreateChildWithSpecifiedIdAsync(Guid parentId, Guid childId, TManyModelCreate item, CancellationToken token = default)
        {
            return _convenience.CreateChildWithSpecifiedIdAsync(parentId, childId, item, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateChildWithSpecifiedIdAndReturnAsync(Guid parentId, Guid childId, TManyModelCreate item,
            CancellationToken token = default)
        {
            return _convenience.CreateChildWithSpecifiedIdAndReturnAsync(parentId, childId, item, token);
        }
    }
}
