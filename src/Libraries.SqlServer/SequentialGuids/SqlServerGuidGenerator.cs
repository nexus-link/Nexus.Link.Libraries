using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuids;

namespace Nexus.Link.Libraries.SqlServer.SequentialGuids;

/// <summary>
/// Optimized Guid generator for SQL Server.
/// </summary>
/// <remarks>
/// Each instance with the same parameters as another instance will use the same generator, i.e.
/// it behaves much like a singleton, but one singleton instance for each parameter combination.
/// </remarks>
public class SqlServerGuidGenerator: IGuidGenerator
{
    private readonly InternalSqlServerGuidGenerator _generator;
    private static readonly Dictionary<int, InternalSqlServerGuidGenerator> GeneratorCache = new();
    private static int? _lastWorkerIndex;
    private static bool _lastUseProcessId;

    public static bool AllowInstancesWithDifferentConfigurations;

    /// <summary>
    /// Each instance with the same parameters as another instance will use the same generator, i.e.
    /// it behaves much like a singleton, but one singleton instance for each parameter combination.
    /// </summary>
    public SqlServerGuidGenerator() : this(0, false)
    {
    } 
    
    /// <summary>
    /// Each instance with the same parameters as another instance will use the same generator, i.e.
    /// it behaves much like a singleton, but one singleton instance for each parameter combination.
    /// </summary>
    /// <param name="workerIndex">??</param>
    /// <param name="useProcessId">??</param>
    /// <remarks>
    /// We don't know much about the implications of the parameters yet, so we recommend going with
    /// the default values.
    /// </remarks>
    protected internal SqlServerGuidGenerator(int workerIndex, bool useProcessId)
    {
        if (!AllowInstancesWithDifferentConfigurations
            && _lastWorkerIndex.HasValue
            && (_lastWorkerIndex.Value != workerIndex || _lastUseProcessId != useProcessId))
        {
            throw new FulcrumContractException(
                $"An instance of {nameof(SqlServerGuidGenerator)} has already been created but with another configuration " +
                $" ({nameof(workerIndex)}={_lastWorkerIndex.Value} and {nameof(useProcessId)}={_lastUseProcessId}. " +
                $"That is not allowed when {nameof(AllowInstancesWithDifferentConfigurations)} is false.");
        }

        _lastWorkerIndex = workerIndex;
        _lastUseProcessId = useProcessId;
        _generator = Factory(workerIndex, useProcessId);
    }

    /// <summary>
    /// Tries to get a <see cref="InternalSqlServerGuidGenerator"/> from a cache, if no one exists,
    /// a new is created and put into the cache.
    /// </summary>
    /// <param name="workerIndex"></param>
    /// <param name="useProcessId"></param>
    private static InternalSqlServerGuidGenerator Factory(int workerIndex = 0, bool useProcessId = false)
    {
        var cacheKey = workerIndex * 2 + (useProcessId ? 1 : 0);
        lock (GeneratorCache)
        {
            if (GeneratorCache.TryGetValue(cacheKey, out var generator)) return generator;
            generator = new InternalSqlServerGuidGenerator(workerIndex, useProcessId);
            GeneratorCache.Add(cacheKey, generator);
            return generator;
        }
    }

    /// <inheritdoc />
    public Guid NewGuid()
    {
        return _generator.NewGuid();
    }
}