using System;
using Nexus.Link.Libraries.Core.Crud.Model;
// ReSharper disable RedundantExtendsListEntry

namespace Nexus.Link.Libraries.Crud.Interfaces
{

    /// <inheritdoc cref="Nexus.Link.Libraries.Crud.Interfaces.ICrudSlaveToMaster{TModelCreate,TModel,TId}" />
    [Obsolete("Use ICrudDependentToMaster. Obsolete since 2021-08-27.")]
    public interface ICrudSlaveToMaster<TModel, TId> :
        ICrudSlaveToMaster<TModel, TModel, TId>,
        ICreateSlave<TModel, TId>,
        ICreateSlaveAndReturn<TModel, TId>,
        ICreateSlaveWithSpecifiedId<TModel, TId>,
        ICrudSlaveToMasterBasic<TModel, TId>
    {
    }


    /// <summary>
    /// Functionality for persisting objects that are "slaves" to another object, i.e. they don't have a life of their own. For instance, if their master is deleted, so should they.
    /// Example: A order item is a slave to an order header.
    /// </summary>
    [Obsolete("Use ICrudDependentToMaster. Obsolete since 2021-08-27.")]
    public interface ICrudSlaveToMaster<in TModelCreate, TModel, TId> :
        ICreateSlave<TModelCreate, TModel, TId>,
        ICreateSlaveAndReturn<TModelCreate, TModel, TId>,
        ICreateSlaveWithSpecifiedId<TModelCreate, TModel, TId>,
        IReadSlave<TModel, TId>,
        IRead<TModel, SlaveToMasterId<TId>>,
        IReadChildrenWithPaging<TModel, TId>,
        IReadChildren<TModel, TId>,
        ISearchChildren<TModel, TId>,
        IUpdateSlave<TModel, TId>,
        IUpdateSlaveAndReturn<TModel, TId>,
        IDeleteSlave<TId>,
        IDeleteChildren<TId>,
#pragma warning disable 618
        ILockableSlave<TId>,
#pragma warning restore 618
        IDistributedLockSlave<TId>,
        ITransactionLockSlave<TModel, TId>,
        ICrudSlaveToMasterBasic<TModelCreate, TModel, TId>
        where TModel : TModelCreate
    {
    }
}
