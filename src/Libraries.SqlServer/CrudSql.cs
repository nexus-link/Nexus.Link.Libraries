﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class CrudSql<TDatabaseItemCreate, TDatabaseItem> : TableBase<TDatabaseItem>, ICrud<TDatabaseItemCreate, TDatabaseItem, Guid>, ISearch<TDatabaseItem, Guid>
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
        public Task<PageEnvelope<TDatabaseItem>> SearchAsync(object condition, int offset = 0, int? limit = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SearchOrderByAsync(condition, null, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchOrderByAsync(object condition, object order, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var whereList = new List<string>();
            var json = JObject.FromObject(condition);
            var token = json.First;
            while (token != null)
            {
                if (!(token is JProperty conditionProperty))
                {
                    throw new FulcrumContractException($"Parameter {nameof(condition)} must be an object with properties:\r{json.ToString(Formatting.Indented)}");
                }

                if (typeof(TDatabaseItem).GetProperty(conditionProperty.Name) == null)
                {
                    throw new FulcrumContractException($"Parameter {nameof(condition)} property {conditionProperty.Name} can't be found in type {typeof(TDatabaseItem).FullName}.");
                }

                whereList.Add($"[{conditionProperty.Name}] = @{conditionProperty.Name}");
                token = token.Next;
            }
            var where = string.Join(", ", whereList);

            
            var orderList = new List<string>();
            string orderBy = null;
            if (order != null)
            {
                json = JObject.FromObject(order);
                token = json.First;
                while (token != null)
                {
                    if (!(token is JProperty conditionProperty))
                    {
                        throw new FulcrumContractException(
                            $"Parameter {nameof(order)} must be an object with properties:\r{json.ToString(Formatting.Indented)}");
                    }

                    if (typeof(TDatabaseItem).GetProperty(conditionProperty.Name) == null)
                    {
                        throw new FulcrumContractException(
                            $"Parameter {nameof(order)} property {conditionProperty.Name} can't be found in type {typeof(TDatabaseItem).FullName}.");
                    }

                    if (conditionProperty.Value.Type != JTokenType.Boolean)
                    {
                        throw new FulcrumContractException($"Parameter {nameof(order)}, property {conditionProperty.Name} must be a boolean.");
                    }

                    var ascending = (bool)conditionProperty.Value;

                    var ascOrDesc = ((bool) conditionProperty.Value) ? "ASC" : "DESC";
                    orderList.Add($"[{conditionProperty.Name}] {ascOrDesc}");
                    token = token.Next;
                }

                orderBy = string.Join(", ", orderList);
            }

            return await SearchWhereAsync(where, orderBy, condition, offset, limit, cancellationToken);
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

        /// <inheritdoc />
        public Task<Lock<Guid>> ClaimDistributedLockAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(Guid id, Guid lockId, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task ClaimTransactionLockAsync(Guid id, CancellationToken token = default(CancellationToken))
        {
            var selectStatement =
                $"SELECT {SqlHelper.ReadColumnList(TableMetadata)} FROM [{TableMetadata.TableName}] WITH (ROWLOCK, UPDLOCK, READPAST) WHERE Id=@Id";
            var result = await SearchAdvancedSingleAsync(selectStatement, new {Id = id}, token);
            if (result == null)
            {
                throw new FulcrumTryAgainException(
                        $"Item {id} in table {TableMetadata.TableName} was already locked by another client.")
                    {
                        RecommendedWaitTimeInSeconds = 1
                    };
            }
        }
    }
}
