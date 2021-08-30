using Nexus.Link.Libraries.Core.Crud.Model;
// ReSharper disable RedundantExtendsListEntry

namespace Nexus.Link.Libraries.Crud.Interfaces
{

    /// <inheritdoc cref="ICrudDependentToMasterWithUniqueId{TModelCreate,TModel,TId}" />
    public interface ICrudDependentToMasterWithUniqueId<TModel, TId, TDependentId> :
        ICrudDependentToMasterWithUniqueId<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMaster<TModel, TId, TDependentId>,
        ICreateDependentAndReturn<TModel, TId, TDependentId>,
        ICrudDependentToMasterWithUniqueIdBasic<TModel, TId, TDependentId>
    {
    }


    /// <summary>
    /// Functionality for persisting objects that are dependent to another object, i.e. they don't have a life of their own. For instance, if their master is deleted, so should they.
    /// Example: Am order item is dependent to an order header.
    /// </summary>
    public interface ICrudDependentToMasterWithUniqueId<in TModelCreate, TModel, TId, TDependentId> :
        ICrudDependentToMaster<TModelCreate, TModel, TId, TDependentId>,
        ICrudDependentToMasterWithUniqueIdBasic<TModelCreate, TModel, TId, TDependentId>,
        IUpdateAndReturn<TModel, TId>,
        IDistributedLock<TId>,
        ITransactionLock<TModel, TId>
        where TModel : TModelCreate
    {
    }
}
