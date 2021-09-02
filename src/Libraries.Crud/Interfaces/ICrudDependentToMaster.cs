using Nexus.Link.Libraries.Core.Crud.Model;
// ReSharper disable RedundantExtendsListEntry

namespace Nexus.Link.Libraries.Crud.Interfaces
{

    /// <inheritdoc cref="ICrudDependentToMaster{TModelCreate,TModel,TId}" />
    public interface ICrudDependentToMaster<TModel, TId, TDependentId> :
        ICrudDependentToMaster<TModel, TModel, TId, TDependentId>,
        ICreateDependent<TModel, TId, TDependentId>,
        ICreateDependentAndReturn<TModel, TId, TDependentId>,
        ICreateDependentWithSpecifiedId<TModel, TId, TDependentId>,
        ICreateDependentWithSpecifiedIdAndReturn<TModel, TId, TDependentId>,
        ICrudDependentToMasterBasic<TModel, TId, TDependentId>
    {
    }


    /// <summary>
    /// Functionality for persisting objects that are dependent to another object, i.e. they don't have a life of their own. For instance, if their master is deleted, so should they.
    /// Example: Am order item is dependent to an order header.
    /// </summary>
    public interface ICrudDependentToMaster<in TModelCreate, TModel, TId, TDependentId> :
        ICreateDependent<TModelCreate, TModel, TId, TDependentId>,
        ICreateDependentAndReturn<TModelCreate, TModel, TId, TDependentId>,
        ICreateDependentWithSpecifiedId<TModelCreate, TModel, TId, TDependentId>,
        ICreateDependentWithSpecifiedIdAndReturn<TModelCreate, TModel, TId, TDependentId>,
        IReadDependent<TModel, TId, TDependentId>,
        IReadChildrenWithPaging<TModel, TId>,
        IReadChildren<TModel, TId>,
        ISearchChildren<TModel, TId>,
        IUpdateDependent<TModel, TId, TDependentId>,
        IUpdateDependentAndReturn<TModel, TId, TDependentId>,
        IDeleteDependent<TId, TDependentId>,
        IDeleteChildren<TId>,
        IDependentDistributedLock<TId, TDependentId>,
        ITransactionLockDependent<TModel, TId, TDependentId>,
        ICrudDependentToMasterBasic<TModelCreate, TModel, TId, TDependentId>
        where TModel : TModelCreate
    {
    }
}
