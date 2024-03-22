using System;

namespace Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuids;

public interface IGuidGenerator
{
    Guid NewGuid();
}