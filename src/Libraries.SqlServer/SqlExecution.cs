using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.Libraries.SqlServer.Model;
using IRecordVersion = Nexus.Link.Libraries.Core.Storage.Model.IRecordVersion;

namespace Nexus.Link.Libraries.SqlServer;

/// <summary>
/// The code for core Queries and Executions.
/// </summary>
public class SqlExecution
{
    private static int _totalExecutionsStarted;
    private static int _totalExecutionsSucceeded;
    private static int _totalQueriesStarted;
    private static int _totalQueriesSucceeded;
    private static int _totalSelects;
    private static int _totalUpdates;
    private static int _totalInserts;
    private static int _totalDeletes;
    private static int _totalOthers;

    public Database Database { get; }
    public ISqlTableMetadata TableMetadata { get; }

    /// <summary>
    /// The total number of <see cref="ExecuteAsync"/> that has started.
    /// </summary>
    public static int TotalExecutionsStarted => _totalExecutionsStarted;

    /// <summary>
    /// The total number of <see cref="ExecuteAsync"/> that has succeeded.
    /// </summary>
    public static int TotalExecutionsSucceeded => _totalExecutionsSucceeded;

    /// <summary>
    /// The total number of <see cref="ExecuteAsync"/> that has failed.
    /// </summary>
    public static int TotalExecutionsFailed => _totalExecutionsStarted - _totalExecutionsSucceeded;


    /// <summary>
    /// The total number of <see cref="InternalQueryAsync{T}"/> that has started.
    /// </summary>
    public static int TotalQueriesStarted => _totalQueriesStarted;

    /// <summary>
    /// The total number of <see cref="InternalQueryAsync{T}"/> that has succeeded.
    /// </summary>
    public static int TotalQueriesSucceeded => _totalQueriesSucceeded;

    /// <summary>
    /// The total number of <see cref="InternalQueryAsync{T}"/> that has failed.
    /// </summary>
    public static int TotalQueriesFailed => _totalQueriesStarted - _totalQueriesSucceeded;

    /// <summary>
    /// The total number of INSERT
    /// </summary>
    public static int TotalInserts => _totalInserts;

    /// <summary>
    /// The total number of SELECT
    /// </summary>
    public static int TotalSelects => _totalSelects;

    /// <summary>
    /// The total number of INSERT
    /// </summary>
    public static int TotalUpdates => _totalUpdates;

    /// <summary>
    /// The total number of SELECT
    /// </summary>
    public static int TotalDeletes => _totalDeletes;

    /// <summary>
    /// The total number of other statements
    /// </summary>
    public static int TotalOthers => _totalOthers;

    public static void ResetCounters()
    {
        _totalExecutionsStarted = 0;
        _totalExecutionsSucceeded = 0;
        _totalQueriesStarted = 0;
        _totalQueriesSucceeded = 0;
        _totalSelects = 0;
        _totalInserts = 0;
        _totalDeletes = 0;
        _totalUpdates = 0;
        _totalOthers = 0;
    }

    protected SqlExecution(ISqlTableMetadata tableMetadata, IDatabaseOptions options)
    {
        Database = new Database(options);
        TableMetadata = tableMetadata;
    }


    protected internal async Task<int> ExecuteAsync(string statement, object param = null, CancellationToken token = default)
    {
        Interlocked.Increment(ref _totalExecutionsStarted);
        IncrementStatementTypeCounters(statement);
        InternalContract.RequireNotNullOrWhiteSpace(statement, nameof(statement));
        MaybeTransformEtagToRecordVersion(param);
        try
        {
            using var db = await Database.NewSqlConnectionAsync(token);
            await db.VerifyAvailabilityAsync(null, token);
            var count = await db.ExecuteAsync(statement, param);
            if (!Database.Options.VerboseLogging) return count;
            var paramAsString = param == null ? "NULL" : JsonConvert.SerializeObject(param);
            Log.LogVerbose($"{statement} {JsonConvert.SerializeObject(paramAsString)}");
            Interlocked.Increment(ref _totalExecutionsSucceeded);
            return count;
        }
        catch (Exception e)
        {
            var paramAsString = param == null ? "NULL" : JsonConvert.SerializeObject(param);
            Log.LogError($"Execution failed:\r{statement}\rwith param:\r{paramAsString}:\r{e.Message}");
            MaybeThrowExceptionAsFulcrumException(e);
            throw;
        }
    }

    protected internal async Task<IEnumerable<T>> InternalQueryAsync<T>(string statement, object param = null, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _totalQueriesStarted);
        IncrementStatementTypeCounters(statement);
        InternalContract.RequireNotNullOrWhiteSpace(statement, nameof(statement));
        MaybeTransformEtagToRecordVersion(param);
        try
        {
            using var db = await Database.NewSqlConnectionAsync(cancellationToken);
            await db.VerifyAvailabilityAsync(null, cancellationToken);
            var items = await db.QueryAsync<T>(statement, param);
            var paramAsString = param == null ? "NULL" : JsonConvert.SerializeObject(param);
            Log.LogVerbose($"{statement} {JsonConvert.SerializeObject(paramAsString)}");
            var itemList = items.ToList();
            foreach (var item in itemList)
            {
                MaybeTransformRecordVersionToEtag(item);
            }
            Interlocked.Increment(ref _totalQueriesSucceeded);
            return itemList;
        }
        catch (Exception e)
        {
            var paramAsString = param == null ? "NULL" : JsonConvert.SerializeObject(param);
            Log.LogError($"Query failed:\r{statement}\rwith param:\r{paramAsString}:\r{e.Message}");
            MaybeThrowExceptionAsFulcrumException(e);
            throw;
        }
    }

    protected void MaybeTransformRecordVersionToEtag(object item)
    {
        if (item is IRecordVersion r && r.RecordVersion != null)
        {
            item.TrySetOptimisticConcurrencyControl(Convert.ToBase64String(r.RecordVersion));
        }
    }

    protected void MaybeTransformEtagToRecordVersion(object item)
    {
        if (item is IRecordVersion r && item.TryGetOptimisticConcurrencyControl(out var eTag) && !string.IsNullOrWhiteSpace(eTag))
        {
            try
            {
                r.RecordVersion = Convert.FromBase64String(eTag);
            }
            catch (Exception)
            {
                // TODO: Get the proper name for the eTag field
                throw new FulcrumConflictException(
                    $"The value in the eTag field ({eTag}) was not a proper value for field {nameof(r.RecordVersion)} of type RowVersion.");
            }
        }
    }

    private void MaybeThrowExceptionAsFulcrumException(Exception e)
    {
        if (e is not SqlException sqlException) return;
        switch (sqlException.Number)
        {
            case (int)SqlConstants.SqlErrorEnum.Deadlock:
                throw new FulcrumTryAgainException(sqlException.Message, sqlException);
            // https://stackoverflow.com/questions/6483699/unique-key-violation-in-sql-server-is-it-safe-to-assume-error-2627
            case (int)SqlConstants.SqlErrorEnum.DuplicateKey:
            case (int)SqlConstants.SqlErrorEnum.UniqueConstraint:
                throw new FulcrumConflictException($"The {TableMetadata.TableName} item must be unique: {e.Message}", e);
            case (int)SqlConstants.SqlErrorEnum.ConstraintFailed:
            case (int)SqlConstants.SqlErrorEnum.CheckConstraintFailed:
                // A complex constraint in the form of a trigger
                throw new FulcrumContractException($"A {TableMetadata.TableName} trigger constraint failed: {e.Message}", e);
        }

        if (sqlException.Number == Database.Options.TriggerConstraintSqlExceptionErrorNumber)
        {
            // A complex constraint in the form of a trigger
            throw new FulcrumContractException($"A {TableMetadata.TableName} trigger constraint failed: {e.Message}", e);
        }
    }

    private static void IncrementStatementTypeCounters(string statement)
    {
        if (statement.StartsWith("SELECT", true, CultureInfo.InvariantCulture))
        {
            Interlocked.Increment(ref _totalSelects);
        }
        else if (statement.StartsWith("UPDATE", true, CultureInfo.InvariantCulture))
        {
            Interlocked.Increment(ref _totalUpdates);
        }
        else if (statement.StartsWith("INSERT", true, CultureInfo.InvariantCulture))
        {
            Interlocked.Increment(ref _totalInserts);
        }
        else if (statement.StartsWith("DELETE", true, CultureInfo.InvariantCulture))
        {
            Interlocked.Increment(ref _totalDeletes);
        }
        else
        {
            Interlocked.Increment(ref _totalOthers);
        }
    }
}