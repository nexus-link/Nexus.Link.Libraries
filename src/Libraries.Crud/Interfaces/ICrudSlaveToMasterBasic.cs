using System;

namespace Nexus.Link.Libraries.Crud.Interfaces
{

    /// <inheritdoc cref="ICrudSlaveToMasterBasic{TModelCreate,TModel,TId}" />
    [Obsolete("Use ICrudDependentToMasterBasic. Obsolete since 2021-08-27.")]
    public interface ICrudSlaveToMasterBasic<TModel, TId> :
        ICrudSlaveToMasterBasic<TModel, TModel, TId>,
        ICreateSlave<TModel, TId>,
        ICreateSlaveWithSpecifiedId<TModel, TId>
    {
    }


    /// <summary>
    /// Functionality for persisting objects that are "slaves" to another object, i.e. they don't have a life of their own. For instance, if their master is deleted, so should they.
    /// Example: A order item is a slave to an order header.
    /// </summary>
    [Obsolete("Use ICrudDependentToMasterBasic. Obsolete since 2021-08-27.")]
    public interface ICrudSlaveToMasterBasic<in TModelCreate, TModel, TId> :
        ICreateSlave<TModelCreate, TModel, TId>,
        IReadSlave<TModel, TId>,
        IReadChildrenWithPaging<TModel, TId>,
        IUpdateSlave<TModel, TId>,
        IDeleteSlave<TId>,
        IDeleteChildren<TId>
        where TModel : TModelCreate
    {
    }
}
