using System;

namespace Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuids;

/// <summary>
/// Use the Guid generator that is built in in .NET.
/// </summary>
public class DotNetGuidGenerator : IGuidGenerator
{
    /// <inheritdoc />
    public Guid NewGuid() => Guid.NewGuid();
}