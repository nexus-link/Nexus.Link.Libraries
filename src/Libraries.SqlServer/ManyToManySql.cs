using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer
{
    public class ManyToManySql<TManyToManyModel, TReferenceModel1, TReferenceModel2> : ManyToManySql<TManyToManyModel, TManyToManyModel, TReferenceModel1, TReferenceModel2>,
        ICrud<TManyToManyModel, Guid>,
        ICrudManyToMany<TManyToManyModel, TReferenceModel1, TReferenceModel2, Guid>
        where TManyToManyModel : class, ITableItem, IValidatable
        where TReferenceModel1 : ITableItem, IValidatable
        where TReferenceModel2 : ITableItem, IValidatable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName1"></param>
        /// <param name="referenceHandler1"></param>
        /// <param name="groupColumnName2"></param>
        /// <param name="referenceHandler2"></param>
        public ManyToManySql(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName1,
            CrudSql<TReferenceModel1> referenceHandler1, string groupColumnName2,
            CrudSql<TReferenceModel2> referenceHandler2)
            : base(connectionString, tableMetadata, groupColumnName1, referenceHandler1, groupColumnName2, referenceHandler2)
        {
        }
    }

    public class ManyToManySql<TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2> : 
        CrudSql<TManyToManyModelCreate, TManyToManyModel>, 
        ICrudManyToMany<TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, Guid>
            where TManyToManyModel : class, TManyToManyModelCreate, ITableItem, IValidatable, IUniquelyIdentifiable<Guid>
            where TReferenceModel1 : ITableItem, IValidatable
            where TReferenceModel2 : ITableItem, IValidatable
    {
        public ManyToOneSql<TManyToManyModel, TReferenceModel1> OneTableHandler1 { get; }
        public ManyToOneSql<TManyToManyModel, TReferenceModel2> OneTableHandler2 { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        /// <param name="groupColumnName1"></param>
        /// <param name="referenceHandler1"></param>
        /// <param name="groupColumnName2"></param>
        /// <param name="referenceHandler2"></param>
        public ManyToManySql(string connectionString, ISqlTableMetadata tableMetadata, string groupColumnName1, CrudSql<TReferenceModel1> referenceHandler1, string groupColumnName2, CrudSql<TReferenceModel2> referenceHandler2)
            : base(connectionString, tableMetadata)
        {
            OneTableHandler1 = new ManyToOneSql<TManyToManyModel, TReferenceModel1>(connectionString, tableMetadata, groupColumnName1, referenceHandler1);
            OneTableHandler2 = new ManyToOneSql<TManyToManyModel, TReferenceModel2>(connectionString, tableMetadata, groupColumnName2, referenceHandler2);
        }

        #region The reference table (the many-to-many table)

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(Guid masterId, Guid slaveId, TManyToManyModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            return CreateAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TManyToManyModel> CreateWithSpecifiedIdAndReturnAsync(Guid masterId, Guid slaveId, TManyToManyModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            return CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public async Task<TManyToManyModel> ReadAsync(Guid reference1Id, Guid reference2Id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(reference1Id, nameof(reference1Id));
            InternalContract.RequireNotDefaultValue(reference2Id, nameof(reference2Id));
            var param = new { Reference1Id = reference1Id, Reference2Id = reference2Id };
            return await SearchWhereSingle($"{OneTableHandler1.ParentColumnName} = @Reference1Id AND {OneTableHandler2.ParentColumnName}= @Reference2Id", param, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TManyToManyModel>> ReadByReference1WithPagingAsync(Guid id, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return await OneTableHandler1.ReadChildrenWithPagingAsync(id, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TManyToManyModel>> ReadByReference1Async(Guid id, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadByReference1WithPagingAsync(id, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TManyToManyModel>> ReadByReference2WithPagingAsync(Guid id, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return await OneTableHandler2.ReadChildrenWithPagingAsync(id, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TManyToManyModel>> ReadByReference2Async(Guid id, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadByReference2WithPagingAsync(id, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Guid masterId, Guid slaveId, TManyToManyModel item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var existingItem = await ReadAsync(masterId, slaveId, token);
            if (Equals(existingItem, default(TManyToManyModel))) throw new FulcrumNotFoundException($"Could not find the item with reference 1 = {masterId} and reference 2 = {slaveId}.");
            var id = existingItem.Id;
            await UpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        public async Task<TManyToManyModel> UpdateAndReturnAsync(Guid masterId, Guid slaveId, TManyToManyModel item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var existingItem = await ReadAsync(masterId, slaveId, token);
            if (Equals(existingItem, default(TManyToManyModel))) throw new FulcrumNotFoundException($"Could not find the item with reference 1 = {masterId} and reference 2 = {slaveId}.");
            var id = existingItem.Id;
            return await UpdateAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(Guid masterId, Guid slaveId, CancellationToken token = new CancellationToken())
        {
            var param = new { Reference1Id = masterId, Reference2Id = slaveId };
            return DeleteWhereAsync(
                $"{OneTableHandler1.ParentColumnName} = @Reference1Id AND {OneTableHandler2.ParentColumnName}= @Reference2Id",
                param, token);
        }

        /// <inheritdoc />
        public async Task DeleteByReference1Async(Guid id, CancellationToken token = default(CancellationToken))
        {
            await OneTableHandler1.DeleteChildrenAsync(id, token);
        }

        /// <inheritdoc />
        public async Task DeleteByReference2Async(Guid id, CancellationToken token = default(CancellationToken))
        {
            await OneTableHandler2.DeleteChildrenAsync(id, token);
        }
        #endregion

        #region The referenced tables

        /// <inheritdoc />
        public async Task<PageEnvelope<TReferenceModel2>> ReadReferencedItemsByReference1WithPagingAsync(Guid id, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return await OneTableHandler2.ReadAllParentsInGroupAsync(
                OneTableHandler1.ParentColumnName,
                id, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TReferenceModel2>> ReadReferencedItemsByReference1Async(Guid id, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadReferencedItemsByReference1WithPagingAsync(id, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TReferenceModel1>> ReadReferencedItemsByReference2WithPagingAsync(Guid id, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return await OneTableHandler1.ReadAllParentsInGroupAsync(
                OneTableHandler2.ParentColumnName,
                id, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TReferenceModel1>> ReadReferencedItemsByReference2Async(Guid id, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadReferencedItemsByReference2WithPagingAsync(id, offset, null, t), limit, token);
        }

        /// <inheritdoc />
        public async Task DeleteReferencedItemsByReference1(Guid id, CancellationToken token = default(CancellationToken))
        {
            await OneTableHandler1.DeleteAllParentsInGroupAsync(OneTableHandler1.ParentColumnName, id, token);
        }

        /// <inheritdoc />
        public async Task DeleteReferencedItemsByReference2(Guid id, CancellationToken token = default(CancellationToken))
        {
            await OneTableHandler1.DeleteAllParentsInGroupAsync(OneTableHandler2.ParentColumnName, id, token);
        }

        #endregion
    }
}
