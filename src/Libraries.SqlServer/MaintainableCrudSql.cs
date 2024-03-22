using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer;

public class MaintainableCrudSql<TDatabaseItem> : MaintainableCrudSql<TDatabaseItem, TDatabaseItem> where TDatabaseItem : IUniquelyIdentifiable<Guid>
{
    public MaintainableCrudSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata) : base(options, tableMetadata)
    {
    }
}

public class MaintainableCrudSql<TDatabaseItemCreate, TDatabaseItem> : CrudSql<TDatabaseItemCreate, TDatabaseItem>, IMaintainableCrudSql where TDatabaseItem : TDatabaseItemCreate, IUniquelyIdentifiable<Guid>
{
    public MaintainableCrudSql(IDatabaseOptions options, ISqlTableMetadata tableMetadata) : base(options, tableMetadata)
    {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(options.ConnectionString);
        DatabaseName = builder.InitialCatalog;
    }


    public async Task<List<MaintenanceStatistics>> GetDatabaseStatisticsAsync()
    {
        var sql = @"
            SELECT dbtables.[name] AS 'Table'
                ,dbindexes.[name] AS 'Index'
                ,indexstats.avg_fragmentation_in_percent as DefragmentationPercentage
                ,CASE 
                    WHEN indexstats.avg_fragmentation_in_percent < 10
                        THEN 'NOTHING'
                    WHEN indexstats.avg_fragmentation_in_percent >= 10
                        AND indexstats.avg_fragmentation_in_percent < 30
                        THEN 'REORGANIZE'
                    WHEN indexstats.avg_fragmentation_in_percent >= 30
                        THEN 'REBUILD'
                    END AS Action
                ,indexstats.page_count as PageCount
            FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, NULL) AS indexstats
            INNER JOIN sys.tables dbtables ON dbtables.[object_id] = indexstats.[object_id]
            INNER JOIN sys.schemas dbschemas ON dbtables.[schema_id] = dbschemas.[schema_id]
            INNER JOIN sys.indexes AS dbindexes ON dbindexes.[object_id] = indexstats.[object_id]
                AND indexstats.index_id = dbindexes.index_id
            WHERE indexstats.database_id = DB_ID()
                AND page_count > 1000
                AND dbindexes.NAME IS NOT NULL
            ORDER BY indexstats.avg_fragmentation_in_percent DESC
                      ";

        using var connection = await Database.NewSqlConnectionAsync();

        // The default command timeout is 30 seconds, but rebuilding an index can take a long time.
        // The intention of these methods are to be called from a timer triggered function, timer triggered web job or similar. These are long running processes but with a maximum timeout of 5 minutes.
        var result = await connection.QueryAsync<MaintenanceStatistics>(sql, commandTimeout: 270);
        return result.ToList();
    }

    /// <summary>
    /// Performs the maintenance job on the database.
    /// </summary>
    /// <param name="maintenanceJob"></param>
    /// <returns></returns>
    public async Task PerformanceMaintenance(MaintenanceStatistics maintenanceJob)
    {
        switch (maintenanceJob.Action)
        {
            case MaintenanceAction.Reorganize:
                await ReorganizeIndexAsync(maintenanceJob.Table, maintenanceJob.Index);
                break;
            case MaintenanceAction.Rebuild:
                await RebuildIndexAsync(maintenanceJob.Table, maintenanceJob.Index);
                break;
        }
    }

    /// <summary>
    /// Updates database statistics after a rebuild / reorganize in order to allow for the most optimal execution plan to be used.
    /// </summary>
    /// <returns></returns>
    public async Task UpdateStatisticsAsync()
    {
        var sql = "EXEC sp_updatestats";
        using var connection = await Database.NewSqlConnectionAsync();
        await connection.ExecuteAsync(sql);
    }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string DatabaseName { get; }

    private async Task ReorganizeIndexAsync(string table, string index)
    {
        var sql = $"ALTER INDEX {index} ON {table} REORGANIZE";

        // The default command timeout is 30 seconds, but rebuilding an index can take a long time.
        // The intention of these methods are to be called from a timer triggered function, timer triggered web job or similar. These are long running processes but with a maximum timeout of 5 minutes.
        using var connection = await Database.NewSqlConnectionAsync();
        await connection.ExecuteAsync(sql, commandTimeout: 270);
    }

    private async Task RebuildIndexAsync(string table, string index)
    {
        var sql = $"ALTER INDEX {index} ON {table} REBUILD WITH (ONLINE = ON)";
        using var connection = await Database.NewSqlConnectionAsync();

        // The default command timeout is 30 seconds, but rebuilding an index can take a long time.
        // The intention of these methods are to be called from a timer triggered function, timer triggered web job or similar. These are long running processes but with a maximum timeout of 5 minutes.
        await connection.ExecuteAsync(sql, commandTimeout: 270);
    }
}