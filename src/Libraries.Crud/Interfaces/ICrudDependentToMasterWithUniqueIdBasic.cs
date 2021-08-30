namespace Nexus.Link.Libraries.Crud.Interfaces
{

    /// <inheritdoc cref="ICrudDependentToMasterWithUniqueIdBasic{TModelCreate,TModel,TId, TDependentId}" />
    public interface ICrudDependentToMasterWithUniqueIdBasic<TModel, TId, TDependentId> :
        ICrudDependentToMasterWithUniqueIdBasic<TModel, TModel, TId, TDependentId>,
        ICreateDependent<TModel, TId, TDependentId>,
        ICreateDependentWithSpecifiedId<TModel, TId, TDependentId>
    {
    }


    /// <summary>
    /// Functionality for persisting objects that are "dependents" to another object, i.e. they don't have a life of their own. For instance, if their master is deleted, so should they.
    /// Example: A order item is a dependent to an order header.
    /// </summary>
    public interface ICrudDependentToMasterWithUniqueIdBasic<in TModelCreate, TModel, TId, TDependentId> :
        ICreateDependent<TModelCreate, TModel, TId, TDependentId>,
        IGetDependentUniqueId<TId, TDependentId>,
        IReadDependent<TModel, TId, TDependentId>,
        IReadChildrenWithPaging<TModel, TId>,
        IRead<TModel, TId>,
        IUpdateDependent<TModel, TId, TDependentId>,
        IDeleteDependent<TId, TDependentId>,
        IUpdate<TModel, TId>,
        IDeleteChildren<TId>,
        IDelete<TId>
        where TModel : TModelCreate
    {
    }
}
