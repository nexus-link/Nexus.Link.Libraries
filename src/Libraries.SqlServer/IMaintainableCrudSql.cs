using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.Libraries.SqlServer;
public interface IMaintainableCrudSql
{
    Task<List<MaintenanceStatistics>> GetDatabaseStatisticsAsync();

    Task PerformanceMaintenance(MaintenanceStatistics maintenanceJob);

    Task UpdateStatisticsAsync();

    string DatabaseName { get; }
}