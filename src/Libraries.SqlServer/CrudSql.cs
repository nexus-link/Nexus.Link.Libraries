using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer
{
    /// <summary>
    /// Helper class for advanced SELECT statements
    /// </summary>
    public class CrudSql<TDatabaseItem> : CrudSql<TDatabaseItem, TDatabaseItem>,
        ICrud<TDatabaseItem, Guid>
        where TDatabaseItem : IUniquelyIdentifiable<Guid>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        public CrudSql(string connectionString, ISqlTableMetadata tableMetadata)
            : base(connectionString, tableMetadata)
        {
            InternalContract.RequireValidated(tableMetadata, nameof(tableMetadata));
        }
    }

    /// <summary>
    /// Helper class for advanced SELECT statements
    /// </summary>
    public class CrudSql<TDatabaseItemCreate, TDatabaseItem> : TableBase<TDatabaseItem>, ICrud<TDatabaseItemCreate, TDatabaseItem, Guid>
        where TDatabaseItem : TDatabaseItemCreate, IUniquelyIdentifiable<Guid>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        public CrudSql(string connectionString, ISqlTableMetadata tableMetadata)
            : base(connectionString, tableMetadata)
        {
            InternalContract.RequireValidated(tableMetadata, nameof(tableMetadata));
        }

        /// <inheritdoc />
        public async Task<Guid> CreateAsync(TDatabaseItemCreate item, CancellationToken token = new CancellationToken())
        {
            var id = Guid.NewGuid();
            await CreateWithSpecifiedIdAsync(id, item, token);
            return id;
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> CreateAndReturnAsync(TDatabaseItemCreate item, CancellationToken token = new CancellationToken())
        {
            var id = Guid.NewGuid();
            return await CreateWithSpecifiedIdAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(Guid id, TDatabaseItemCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            var dbItem = StorageHelper.DeepCopy<TDatabaseItem, TDatabaseItemCreate>(item);
            dbItem.Id = id;
            StorageHelper.MaybeCreateNewEtag(dbItem);
            StorageHelper.MaybeUpdateTimeStamps(dbItem, true);
            StorageHelper.MaybeValidate(dbItem);
            var sql = SqlHelper.Create(TableMetadata);
            await ExecuteAsync(sql, dbItem, token);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> CreateWithSpecifiedIdAndReturnAsync(Guid id, TDatabaseItemCreate item,
            CancellationToken token = new CancellationToken())
        {
            await CreateWithSpecifiedIdAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            await DeleteWhereAsync("Id=@Id", new { Id = id }, token);
        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            await DeleteWhereAsync("1=1", null, token);
        }

        public async Task<PageEnvelope<TDatabaseItem>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return await SearchAllAsync(null, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TDatabaseItem>> ReadAllAsync(int limit = 2147483647, CancellationToken token = new CancellationToken())
        {
            return await StorageHelper.ReadPagesAsync<TDatabaseItem>(
                (offset, cancellationToken) => ReadAllWithPagingAsync(offset, null, cancellationToken),
                limit, token);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> ReadAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return await SearchWhereSingle("Id = @Id", new { Id = id }, token);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Guid id, TDatabaseItem item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(item, nameof(item));
            StorageHelper.MaybeValidate(item);
            await InternalUpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        public Task<TDatabaseItem> UpdateAndReturnAsync(Guid id, TDatabaseItem item, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        protected async Task InternalUpdateAsync(Guid id, TDatabaseItem item, CancellationToken token)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            StorageHelper.MaybeValidate(item);
            await MaybeVerifyEtagForUpdateAsync(id, item, token);
            using (var db = Database.NewSqlConnection())
            {
                if (item is IOptimisticConcurrencyControlByETag etaggable)
                {
                    var sql = SqlHelper.Update(TableMetadata, etaggable.Etag);
                    var count = await db.ExecuteAsync(sql, item);
                    if (count == 0)
                        throw new FulcrumConflictException(
                            "Could not update. Your data was stale. Please reload and try again.");
                }
                else
                {
                    var sql = SqlHelper.Update(TableMetadata);
                    await db.ExecuteAsync(sql, item);
                }
            }
        }

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IOptimisticConcurrencyControlByETag"/>
        /// then the old value is read using <see cref="ReadBase{TModel,TId}.ReadAsync"/> and the values are verified to be equal.
        /// The Etag of the item is then set to a new value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        protected virtual async Task MaybeVerifyEtagForUpdateAsync(Guid id, TDatabaseItem item, CancellationToken token = default(CancellationToken))
        {
            if (item is IOptimisticConcurrencyControlByETag etaggable)
            {
                var oldItem = await ReadAsync(id, token);
                if (oldItem != null)
                {
                    var oldEtag = (oldItem as IOptimisticConcurrencyControlByETag)?.Etag;
                    if (oldEtag?.ToLowerInvariant() != etaggable.Etag?.ToLowerInvariant())
                        throw new FulcrumConflictException($"The updated item ({item}) had an old ETag value.");
                }
            }
        }

        /// <inheritdoc />
        public Task<Lock<Guid>> ClaimLockAsync(Guid id, CancellationToken token = new CancellationToken())
        {
            throw new FulcrumNotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(Guid id, Guid lockId, CancellationToken token = new CancellationToken())
        {
            throw new FulcrumNotImplementedException();
        }
    }
}
