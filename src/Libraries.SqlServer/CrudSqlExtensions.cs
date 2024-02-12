using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;

namespace Nexus.Link.Libraries.SqlServer;

public static class CrudSqlExtensions
{
    public static T CreateNewId<T>(this SqlExecution sql)
    {
        GuidOptimization optimization = sql.Database.Options.GuidOptimization ?? StorageHelper.Optimization;
        return StorageHelper.CreateNewId<T>(optimization);
    }
}