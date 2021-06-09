using System;
using System.Collections.Generic;
using System.Linq;
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
        protected CrudSql<TOneModel> OneTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="oneTableHandler"></param>
        public ManyToOneSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName,
            CrudSql<TOneModel> oneTableHandler)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            OneTableHandler = oneTableHandler;
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
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await OneTableHandler.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.GetOrderBy("many"), new { ColumnValue = groupColumnValue }, offset, limit, token);
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
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            await OneTableHandler.ExecuteAsync(deleteStatement, new { ColumnValue = groupColumnValue }, token);
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
    }

    public class ManyToOneSql<TManyModelCreate, TManyModel, TOneModel> :
        CrudSql<TManyModelCreate, TManyModel>,
        ICrudManyToOne<TManyModelCreate, TManyModel, Guid>
            where TManyModel : TManyModelCreate, IUniquelyIdentifiable<Guid>, new()
            where TOneModel : IUniquelyIdentifiable<Guid>
    {
        private readonly ManyToOneConvenience<TManyModelCreate, TManyModel, Guid> _convenience;

        public string ParentColumnName { get; }
        protected CrudSql<TOneModel> OneTableHandler { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="parentColumnName"></param>
        /// <param name="oneTableHandler"></param>
        public ManyToOneSql(string connectionString, ISqlTableMetadata tableMetadata, string parentColumnName, CrudSql<TOneModel> oneTableHandler)
            : base(connectionString, tableMetadata)
        {
            ParentColumnName = parentColumnName;
            OneTableHandler = oneTableHandler;
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
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            return await OneTableHandler.SearchAdvancedAsync("SELECT COUNT(one.[Id])", "SELECT one.*", selectRest, TableMetadata.GetOrderBy("many"), new { ColumnValue = groupColumnValue }, offset, limit, token);
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
                             $" JOIN [{OneTableHandler.TableName}] AS one ON (one.Id = many.[{ParentColumnName}])" +
                             $" WHERE [{groupColumnName}] = @ColumnValue";
            await OneTableHandler.ExecuteAsync(deleteStatement, new { ColumnValue = groupColumnValue }, token);
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
    }
}
