using System;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Operations for reading and deleting the children of a parent.
    /// </summary>
    [Obsolete("Use IDependentToMaster. Obsolete since 2021-08-27.")]
    public interface ISlaveToMaster<TModel, in TId> :
        IReadChildrenWithPaging<TModel, TId>,
        IDeleteChildren<TId>
    {
    }
}
