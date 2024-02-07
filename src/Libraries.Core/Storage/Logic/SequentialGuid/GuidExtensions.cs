using System;

namespace Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;

public class GuidExtensions
{
    /// <summary>
    /// Creates a sequential GUID according to <see crStorageHelperages.Optimization"/> ordering rules.
    /// </summary>
    public static Guid NewSequentialGuid()
    {
        return StorageHelper.Optimization is GuidOptimization.None ? System.Guid.NewGuid() : StorageHelper.CreateNewId<Guid>();
    }
}