using System;

namespace Nexus.Link.Libraries.Crud.Mappers
{
    /// <summary>
    /// Indicates that the implementor has one or more crud methods./>.
    /// </summary>
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public interface IMappable
    {
    }

    /// <inheritdoc />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public interface IMappable<in TModel, in TId> : IMappable
    {
    }
}
