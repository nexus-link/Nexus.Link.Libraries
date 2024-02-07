using System;

namespace Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;

public interface IGuidGenerator
{
    Guid NewSequentialGuid();
}