using System;

namespace Nexus.Link.Libraries.Crud.Mappers
{
    /// <summary>
    /// Indicates that the implementor has one or more crud methods./>.
    /// </summary>
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete since 2020-09-23.")]
    public interface IMappable
    {
    }

    /// <inheritdoc />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete since 2020-09-23.")]
    public interface IMappable<in TModel, in TId> : IMappable
    {
    }
}
