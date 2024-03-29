﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer
{
    /// <summary>
    /// Helper class for advanced SELECT statements
    /// </summary>
    /// <typeparam name="TDatabaseItem"></typeparam>
    public abstract class TableBase<TDatabaseItem> : SqlExecution, ISearch<TDatabaseItem>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableMetadata"></param>
        [Obsolete("Use TableBase(IDatabaseOptions, ISqlTableMetadata) instead. Obsolete since 2021-10-21.", error: false)]
        protected TableBase(string connectionString, ISqlTableMetadata tableMetadata) : this(new DatabaseOptions() { ConnectionString = connectionString }, tableMetadata)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="tableMetadata"></param>
        protected TableBase(IDatabaseOptions options, ISqlTableMetadata tableMetadata)
        :base(tableMetadata, options)
        {
            InternalContract.RequireNotNull(options, nameof(options));
            InternalContract.RequireValidated(tableMetadata, nameof(tableMetadata));
        }

        /// <summary>
        /// The name of the table that this class handles.
        /// </summary>
        public string TableName => TableMetadata.TableName;

        protected async Task DeleteWhereAsync(string where = null, object param = null, CancellationToken token = default)
        {
            where = string.IsNullOrWhiteSpace(where) ? "1=1" : where;
            await ExecuteAsync(SqlHelper.Delete(TableMetadata, where), param, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAllAsync(string orderBy, int offset, int? limit = null,
            CancellationToken token = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null) InternalContract.RequireGreaterThanOrEqualTo(0, limit.Value, nameof(limit));
            return await SearchWhereAsync(null, orderBy, null, offset, limit, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchWhereAsync(string where = null, string orderBy = null, object param = null, int offset = 0, int? limit = null, CancellationToken token = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit.Value, nameof(limit));
            var total = await CountItemsWhereAsync(where, param, token);
            var pageEnvelope = new PageEnvelope<TDatabaseItem>
            {
                Data = Enumerable.Empty<TDatabaseItem>(),
                PageInfo = new PageInfo
                {
                    Offset = offset,
                    Limit = limit.Value,
                    Returned = 0,
                    Total = total
                }
            };
            if (total == 0) return pageEnvelope;
            var data = await InternalSearchWhereAsync(param, where, orderBy, offset, limit.Value, token);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            pageEnvelope.Data = dataAsArray;
            pageEnvelope.PageInfo.Returned = dataAsArray.Length;
            return pageEnvelope;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAndLockWhereAsync(string where = null, string orderBy = null, object param = null, int offset = 0, int? limit = null, CancellationToken token = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit.Value, nameof(limit));
            var total = await CountItemsAdvancedAsync("SELECT COUNT(*)", $"FROM [{TableMetadata.TableName}] WITH (NOLOCK) WHERE ({@where})", param, token);
            var pageEnvelope = new PageEnvelope<TDatabaseItem>
            {
                Data = Enumerable.Empty<TDatabaseItem>(),
                PageInfo = new PageInfo
                {
                    Offset = offset,
                    Limit = limit.Value,
                    Returned = 0,
                    Total = total
                }
            };
            if (total == 0) return pageEnvelope;
            var data = await InternalSearchAndLockWhereAsync(param, where, orderBy, offset, limit.Value, token);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            pageEnvelope.Data = dataAsArray;
            pageEnvelope.PageInfo.Returned = dataAsArray.Length;
            return pageEnvelope;
        }

        /// <inheritdoc />
        public async Task<int> CountItemsWhereAsync(string where = null, object param = null, CancellationToken token = default)
        {
            where = where ?? "1=1";
            return await CountItemsAdvancedAsync("SELECT COUNT(*)", $"FROM [{TableMetadata.TableName}] WHERE ({@where})", param, token);
        }

        /// <inheritdoc />
        public async Task<int> CountItemsAdvancedAsync(string selectFirst, string selectRest, object param = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(selectFirst, nameof(selectFirst));
            InternalContract.RequireNotNullOrWhiteSpace(selectRest, nameof(selectRest));
            if (selectRest == null) return 0;
            var selectStatement = $"{selectFirst} {selectRest}";
            var result = await InternalQueryAsync<int>(selectStatement, param, cancellationToken);
            FulcrumAssert.IsNotNull(result, CodeLocation.AsString());
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        [Obsolete("Please use SearchSingleWhereAsync. Obsolete since 2022-01-03.")]
        public Task<TDatabaseItem> SearchWhereSingle(string where, object param = null, CancellationToken token = default)
        {
            return SearchSingleWhereAsync(where, param, token);
        }

        public Task<TDatabaseItem> SearchSingleWhereAsync(string @where, object param = null, CancellationToken token = default)
        {
            if (where == null) where = "1=1";
            return SearchAdvancedSingleAsync($"SELECT * FROM [{TableMetadata.TableName}] WHERE ({where})", param, token);
        }

        public async Task<TDatabaseItem> SearchSingleAndLockWhereAsync(string @where, object param = null, CancellationToken token = default)
        {
            if (where == null) where = "1=1";
            var item = await SearchAdvancedSingleAsync($"SELECT * FROM [{TableMetadata.TableName}] WITH (ROWLOCK, UPDLOCK, READPAST) WHERE ({where})", param, token);
            if (item != null) return item;
            var count = await CountItemsAdvancedAsync("SELECT COUNT(*)", $"FROM [{TableMetadata.TableName}] WITH (NOLOCK) WHERE ({@where})", param, token);
            if (count == 0) return default;
            throw new FulcrumResourceLockedException($"The specified row in table {TableMetadata.TableName} was already locked")
            {
                RecommendedWaitTimeInSeconds = 1.0
            };
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchAdvancedSingleAsync(string selectStatement, object param = null, CancellationToken token = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(selectStatement, nameof(selectStatement));
            return await SearchFirstAdvancedAsync(selectStatement, null, param, token);
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstWhereAsync(string where = null, string orderBy = null, object param = null, CancellationToken token = default)
        {
            var result = await InternalSearchWhereAsync(param, where, orderBy, 0, 1, token);
            return result.SingleOrDefault();
        }

        public async Task<TDatabaseItem> SearchAndLockFirstWhereAsync(string @where = null, string orderBy = null, object param = null,
            CancellationToken token = default)
        {
            var result = await InternalSearchAndLockWhereAsync(param, where, orderBy, 0, 1, token);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<TDatabaseItem> SearchFirstAdvancedAsync(string selectStatement, string orderBy = null, object param = null, CancellationToken token = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(selectStatement, nameof(selectStatement));
            var result = await InternalSearchAsync(param, selectStatement, orderBy, 0, 1, token);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TDatabaseItem>> SearchAdvancedAsync(string countFirst, string selectFirst, string selectRest, string orderBy = null, object param = null, int offset = 0, int? limit = null, CancellationToken token = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit.Value, nameof(limit));
            var total = await CountItemsAdvancedAsync(countFirst, selectRest, param, token);
            var pageEnvelope = new PageEnvelope<TDatabaseItem>
            {
                Data = Enumerable.Empty<TDatabaseItem>(),
                PageInfo = new PageInfo
                {
                    Offset = offset,
                    Limit = limit.Value,
                    Returned = 0,
                    Total = total
                }
            };
            if (total == 0) return pageEnvelope;
            var selectStatement = selectRest == null ? null : $"{selectFirst} {selectRest}";
            var data = await InternalSearchAsync(param, selectStatement, orderBy, offset, limit.Value, token);
            var dataAsArray = data as TDatabaseItem[] ?? data.ToArray();
            pageEnvelope.Data = dataAsArray;
            pageEnvelope.PageInfo.Returned = dataAsArray.Length;
            return pageEnvelope;
        }

        /// <summary>
        /// Find the items specified by the <paramref name="where"/> clause.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> condition.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        protected async Task<IEnumerable<TDatabaseItem>> InternalSearchWhereAsync(object param, string where, string orderBy,
            int offset, int limit, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            where = where ?? "1=1";
            return await InternalSearchAsync(param,
                $"SELECT *" +
                $" FROM [{TableMetadata.TableName}]" +
                $" WHERE ({where})",
                orderBy, offset, limit, cancellationToken);
        }

        /// <summary>
        /// Find and lock the items specified by the <paramref name="where"/> clause.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="where"/> condition.</param>
        /// <param name="where">The search condition for the SELECT statement.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        protected async Task<IEnumerable<TDatabaseItem>> InternalSearchAndLockWhereAsync(object param, string where, string orderBy,
            int offset, int limit, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            where = where ?? "1=1";
            return await InternalSearchAsync(param,
                $"SELECT *" +
                $" FROM [{TableMetadata.TableName}]" +
                " WITH (ROWLOCK, UPDLOCK, READPAST)" +
                $" WHERE ({where})",
                orderBy, offset, limit, cancellationToken);
        }

        /// <summary>
        /// Find the items specified by the <paramref name="selectStatement"/>.
        /// </summary>
        /// <param name="param">The fields for the <paramref name="selectStatement"/> condition.</param>
        /// <param name="selectStatement">The SELECT statement, including WHERE, but not ORDER BY.</param>
        /// <param name="orderBy">An expression for how to order the result.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The found items.</returns>
        /// 
        protected async Task<IEnumerable<TDatabaseItem>> InternalSearchAsync(object param, string selectStatement, string orderBy,
            int offset, int limit, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThanOrEqualTo(0, limit, nameof(limit));
            InternalContract.RequireNotNullOrWhiteSpace(selectStatement, nameof(selectStatement));
            orderBy = orderBy ?? TableMetadata.GetOrderBy() ?? "1";
            var sqlQuery = $"{selectStatement} " +
                           $" ORDER BY {orderBy}" +
                           $" OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY";
            return await QueryAsync(sqlQuery, param, cancellationToken);
        }

        protected internal Task<IEnumerable<TDatabaseItem>> QueryAsync(string statement, object param = null, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(statement, nameof(statement));
            return InternalQueryAsync<TDatabaseItem>(statement, param, cancellationToken);
        }
    }
}
