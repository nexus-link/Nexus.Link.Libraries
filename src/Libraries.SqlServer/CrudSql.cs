using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.Libraries.SqlServer.Model;
using IRecordVersion = Nexus.Link.Libraries.Core.Storage.Model.IRecordVersion;

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
        [Obsolete("Use CrudSql(IDatabaseOptions, ISqlTableMetadata) instead. Obsolete since 2021-10-21.", error: false)]
        public CrudSql(string connectionString, ISqlTableMetadata tableMetadata)
            : base(connectionString, tableMetadata)
        {
            InternalContract.RequireValidated(tableMetadata, nameof(tableMetadata));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CrudSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata)
            : base(options, tableMetadata)
        {
        }
    }

    /// <summary>
    /// Helper class for advanced SELECT statements
    /// </summary>
    public class CrudSql<TDatabaseItemCreate, TDatabaseItem> :
        TableBase<TDatabaseItem>, ICrud<TDatabaseItemCreate, TDatabaseItem, Guid>,
        ISearch<TDatabaseItem, Guid>
        where TDatabaseItem : TDatabaseItemCreate, IUniquelyIdentifiable<Guid>
    {
        private readonly CrudConvenience<TDatabaseItemCreate, TDatabaseItem, Guid> _convenience;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        [Obsolete("Use CrudSql(IDatabaseOptions, ISqlTableMetadata) instead. Obsolete since 2021-10-21.", error: false)]
        public CrudSql(string connectionString, ISqlTableMetadata tableMetadata)
            : base(connectionString, tableMetadata)
        {
            InternalContract.RequireValidated(tableMetadata, nameof(tableMetadata));
            _convenience = new CrudConvenience<TDatabaseItemCreate, TDatabaseItem, Guid>(this);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CrudSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata)
            : base(options, tableMetadata)
        {
            _convenience = new CrudConvenience<TDatabaseItemCreate, TDatabaseItem, Guid>(this);
        }

        /// <inheritdoc />
        public async Task<Guid> CreateAsync(TDatabaseItemCreate item, CancellationToken token = default)
        {
            var id = Guid.NewGuid();
            await CreateWithSpecifiedIdAsync(id, item, token);
            return id;
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> CreateAndReturnAsync(TDatabaseItemCreate item, CancellationToken token = default)
        {
            var id = Guid.NewGuid();
            return await CreateWithSpecifiedIdAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(Guid id, TDatabaseItemCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            var dbItem = PrepareDbItem(id, item);
            InternalContract.RequireValidated(dbItem, nameof(item));
            var sql = SqlHelper.Create(TableMetadata);
            await ExecuteAsync(sql, dbItem, token);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> CreateWithSpecifiedIdAndReturnAsync(Guid id, TDatabaseItemCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            InternalContract.RequireNotNull(item, nameof(item));
            if (!TableMetadata.InsertCanUseOutput)
            {
                await CreateWithSpecifiedIdAsync(id, item, token);
                return await ReadAsync(id, token);
            }

            var dbItem = PrepareDbItem(id, item);
            InternalContract.RequireValidated(dbItem, nameof(item));
            var sql = SqlHelper.CreateAndRead(TableMetadata);

            var items = await QueryAsync(sql, dbItem, token);
            FulcrumAssert.IsNotNull(items);
            var array = items.ToArray();
            FulcrumAssert.AreEqual(1, array.Length);
            return array[0];
        }

        private TDatabaseItem PrepareDbItem(Guid id, TDatabaseItemCreate item)
        {
            var dbItem = StorageHelper.DeepCopy<TDatabaseItem, TDatabaseItemCreate>(item);
            dbItem.Id = id;
            if (dbItem.TryGetOptimisticConcurrencyControl(out var eTag))
            {
                if (dbItem is IRecordVersion r)
                {
                    // This is to set Etag to a fictive value that will never be used (but may be required by the validation below)
                    r.RecordVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    MaybeTransformRecordVersionToEtag(dbItem);
                }
                else
                {
                    dbItem.TrySetOptimisticConcurrencyControl();
                }
            }
            StorageHelper.MaybeUpdateTimeStamps(dbItem, true);
            return dbItem;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            await DeleteWhereAsync("Id=@Id", new { Id = id }, token);
        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken token = default)
        {
            await DeleteWhereAsync("1=1", null, token);
        }

        public Task<PageEnvelope<TDatabaseItem>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            return SearchAllAsync(null, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAsync(SearchDetails<TDatabaseItem> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            var where = CrudSearchHelper.GetWhereStatement(details);
            var orderBy = CrudSearchHelper.GetOrderByStatement(details) ?? TableMetadata.GetOrderBy();

            return await SearchWhereAsync(where, orderBy, details.GetWhereAsModel("%", "_"), offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TDatabaseItem> FindUniqueAsync(SearchDetails<TDatabaseItem> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TDatabaseItem>> ReadAllAsync(int limit = 2147483647, CancellationToken token = default)
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return await StorageHelper.ReadPagesAsync<TDatabaseItem>(
                (offset, cancellationToken) => ReadAllWithPagingAsync(offset, null, cancellationToken),
                limit, token);
        }

        /// <inheritdoc />
        public Task<TDatabaseItem> ReadAsync(Guid id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return SearchSingleWhereAsync("Id = @Id", new { Id = id }, token);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Guid id, TDatabaseItem item, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            string sql;
            switch (item)
            {
                case IRecordVersion _:
                    sql = SqlHelper.UpdateIfSameRowVersion(TableMetadata);
                    break;
                default:
                    if (item.TryGetOptimisticConcurrencyControl(out var oldEtag))
                    {
                        item.TrySetOptimisticConcurrencyControl();
                        sql = SqlHelper.UpdateIfSameEtag(TableMetadata, oldEtag);
                    }
                    else
                    {
                        sql = SqlHelper.Update(TableMetadata);
                    }

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
        public async Task<TDatabaseItem> UpdateAndReturnAsync(Guid id, TDatabaseItem item, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));

            if (!TableMetadata.UpdateCanUseOutput)
            {
                await UpdateAsync(id, item, token);
                return await ReadAsync(id, token);
            }

            string sql;
            switch (item)
            {
                case IRecordVersion _:
                    sql = SqlHelper.UpdateIfSameRowVersionAndRead(TableMetadata);
                    break;
                default:
                    if (item.TryGetOptimisticConcurrencyControl(out var oldEtag))
                    {
                        item.TrySetOptimisticConcurrencyControl();
                        sql = SqlHelper.UpdateIfSameEtagAndRead(TableMetadata, oldEtag);
                    }
                    else
                    {
                        sql = SqlHelper.UpdateAndRead(TableMetadata);
                    }
                    break;
            }

            var items = await QueryAsync(sql, item, token);
            FulcrumAssert.IsNotNull(items);
            var array = items.ToArray();
            if (array.Length == 0)
            {
                if (item is IRecordVersion || item.TryGetOptimisticConcurrencyControl(out _))
                {
                    throw new FulcrumConflictException(
                        "Could not update. Your data was stale. Please reread the data and try to update it again.");
                }

                throw new FulcrumNotFoundException($"Could not update, no record with Id={item.Id} found.");
            }
            FulcrumAssert.AreEqual(1, array.Length, CodeLocation.AsString());
            return array[0];
        }

        /// <inheritdoc />
        public Task<Lock<Guid>> ClaimLockAsync(Guid id, CancellationToken token = default)
        {
            throw new FulcrumNotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(Guid id, Guid lockId, CancellationToken token = default)
        {
            throw new FulcrumNotImplementedException();
        }

        /// <inheritdoc />
        public Task<Lock<Guid>> ClaimDistributedLockAsync(Guid id, TimeSpan? lockTimeSpan = null, Guid currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            var lockTable = Database.Options.DistributedLockTable;
            if (lockTable == null)
            {
                throw new FulcrumContractException(
                    $"You must set {nameof(IDatabaseOptions)}.{nameof(IDatabaseOptions.DistributedLockTable)} to use distributed locks.");
            }

            return lockTable.TryAddAsync(id, Database.Options.DefaultLockTimeSpan, currentLockId, cancellationToken);
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(Guid id, Guid lockId, CancellationToken cancellationToken = default)
        {
            var lockTable = Database.Options.DistributedLockTable;
            if (lockTable == null)
            {
                throw new FulcrumContractException(
                    $"You must set {nameof(IDatabaseOptions)}.{nameof(IDatabaseOptions.DistributedLockTable)} to use distributed locks.");
            }
            return lockTable.RemoveAsync(id, lockId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task ClaimTransactionLockAsync(Guid id, CancellationToken token = default)
        {
            var result = await SearchSingleAndLockWhereAsync("Id=@Id", new { Id = id }, token);
            if (result == null)
            {
                throw new FulcrumResourceLockedException(
                        $"Item {id} in table {TableMetadata.TableName} was already locked by another client.")
                {
                    RecommendedWaitTimeInSeconds = 1
                };
            }
        }

        /// <inheritdoc />
        public Task<TDatabaseItem> ClaimTransactionLockAndReadAsync(Guid id, CancellationToken token = default)
        {
            return SearchSingleAndLockWhereAsync("Id=@Id", new { Id = id }, token);
        }
    }
}
