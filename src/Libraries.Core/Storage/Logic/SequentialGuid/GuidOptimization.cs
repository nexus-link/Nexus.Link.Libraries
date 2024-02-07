namespace Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;

public enum GuidOptimization
{
    None,
    SqlServer,
    SqlServerWithProcessId,
    MySql,
    RavenDb,
    Oracle
}