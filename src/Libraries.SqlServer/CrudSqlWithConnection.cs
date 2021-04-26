using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
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
    public class CrudSqlWithConnection<TDatabaseItem> : CrudSqlWithConnection<TDatabaseItem, TDatabaseItem>,
        ICrudWithConnection<TDatabaseItem, Guid>
        where TDatabaseItem : IUniquelyIdentifiable<Guid>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        public CrudSqlWithConnection(string connectionString, ISqlTableMetadata tableMetadata)
            : base(connectionString, tableMetadata)
        {
            InternalContract.RequireValidated(tableMetadata, nameof(tableMetadata));
        }
    }

    /// <summary>
    /// Helper class for advanced SELECT statements
    /// </summary>
    public class CrudSqlWithConnection<TDatabaseItemCreate, TDatabaseItem> : TableBase<TDatabaseItem>, ICrudWithConnection<TDatabaseItemCreate, TDatabaseItem, Guid>
        where TDatabaseItem : TDatabaseItemCreate, IUniquelyIdentifiable<Guid>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        public CrudSqlWithConnection(string connectionString, ISqlTableMetadata tableMetadata)
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
            if (dbItem is IOptimisticConcurrencyControlByETag)
            {
                if (dbItem is IRecordVersion r)
                {
                    // This is to set Etag to a fictive value that will never be used (but may be required by the validation below)
                    r.RecordVersion = new byte[]{0,0,0,0,0,0,0,0};
                    MaybeTransformRecordVersionToEtag(dbItem);
                }
                else
                {
                    StorageHelper.MaybeCreateNewEtag(dbItem);
                }
            }
            StorageHelper.MaybeUpdateTimeStamps(dbItem, true);
            InternalContract.RequireValidated(dbItem, nameof(item));
            var sql = SqlHelper.Create(TableMetadata);
            var count = await ExecuteAsync(sql, dbItem, token);
            FulcrumAssert.AreEqual(1, count, CodeLocation.AsString());
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

        public Task<PageEnvelope<TDatabaseItem>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return SearchAllAsync(null, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TDatabaseItem>> ReadAllAsync(int limit = 2147483647, CancellationToken token = new CancellationToken())
        {
            return await StorageHelper.ReadPagesAsync<TDatabaseItem>(
                (offset, cancellationToken) => ReadAllWithPagingAsync(offset, null, cancellationToken),
                limit, token);
        }

        /// <inheritdoc />
        public Task<TDatabaseItem> ReadAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return SearchWhereSingle("Id = @Id", new { Id = id }, token);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Guid id, TDatabaseItem item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await InternalUpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> UpdateAndReturnAsync(Guid id, TDatabaseItem item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            await UpdateAsync(id, item, token);
            return await ReadAsync(id, token);
        }

        protected async Task InternalUpdateAsync(Guid id, TDatabaseItem item, CancellationToken token)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            string sql;
            switch (item)
            {
                case IRecordVersion _:
                    sql = SqlHelper.UpdateIfSameRowVersion(TableMetadata);
                    break;
                case IOptimisticConcurrencyControlByETag o:
                    var oldEtag = o.Etag;
                    StorageHelper.MaybeCreateNewEtag(o);
                    sql = SqlHelper.UpdateIfSameEtag(TableMetadata, oldEtag);
                    break;
                default:
                    sql = SqlHelper.Update(TableMetadata);
                    break;
            }

            var count = await ExecuteAsync(sql, item, token);

            if (count == 0)
            {
                if (item is IRecordVersion || item is IOptimisticConcurrencyControlByETag)
                {
                    throw new FulcrumConflictException(
                        "Could not update. Your data was stale. Please reread the data and try to update it again.");
                }

                throw new FulcrumNotFoundException(
                    $"Could not update, no record with Id={item.Id} found.");
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

        public async Task<Guid> CreateAsync(TDatabaseItemCreate item, IDbConnection connection,
            CancellationToken token = default(CancellationToken))
        {
            var id = Guid.NewGuid();
            await CreateWithSpecifiedIdAsync(id, item, connection, token);
            return id;
        }

        public async Task CreateWithSpecifiedIdAsync(Guid id, TDatabaseItemCreate item, IDbConnection connection,
            CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            var dbItem = StorageHelper.DeepCopy<TDatabaseItem, TDatabaseItemCreate>(item);
            dbItem.Id = id;
            if (dbItem is IOptimisticConcurrencyControlByETag)
            {
                if (dbItem is IRecordVersion r)
                {
                    // This is to set Etag to a fictive value that will never be used (but may be required by the validation below)
                    r.RecordVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    MaybeTransformRecordVersionToEtag(dbItem);
                }
                else
                {
                    StorageHelper.MaybeCreateNewEtag(dbItem);
                }
            }
            StorageHelper.MaybeUpdateTimeStamps(dbItem, true);
            InternalContract.RequireValidated(dbItem, nameof(item));
            var sql = SqlHelper.Create(TableMetadata);
            var count = await ExecuteAsync(sql, dbItem, token, connection);
            FulcrumAssert.AreEqual(1, count, CodeLocation.AsString());
        }

        public async Task<TDatabaseItem> CreateWithSpecifiedIdAndReturnAsync(Guid id, TDatabaseItemCreate item, IDbConnection connection,
            CancellationToken token = default(CancellationToken))
        {
            await CreateWithSpecifiedIdAsync(id, item, connection, token);
            return await ReadAsync(id, token);
        }

        public IDbConnection CreateConnection()
        {
            return Database.NewSqlConnection();
        }
    }
}
