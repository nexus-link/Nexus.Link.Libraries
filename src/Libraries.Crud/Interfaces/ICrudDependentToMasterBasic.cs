namespace Nexus.Link.Libraries.Crud.Interfaces
{

    /// <inheritdoc cref="ICrudDependentToMasterBasic{TModelCreate,TModel,TId, TDependentId}" />
    public interface ICrudDependentToMasterBasic<TModel, TId, in TDependentId> :
        ICrudDependentToMasterBasic<TModel, TModel, TId, TDependentId>,
        ICreateDependentWithSpecifiedId<TModel, TId, TDependentId>
    {
    }


    /// <summary>
    /// Functionality for persisting objects that are "dependents" to another object, i.e. they don't have a life of their own. For instance, if their master is deleted, so should they.
    /// Example: A order item is a dependent to an order header.
    /// </summary>
    public interface ICrudDependentToMasterBasic<in TModelCreate, TModel, TId, in TDependentId> :
        IGetDependentUniqueId<TId, TDependentId>,
        IReadDependent<TModel, TId, TDependentId>,
        IReadChildrenWithPaging<TModel, TId>,
        IUpdateDependent<TModel, TId, TDependentId>,
        IDeleteDependent<TId, TDependentId>,
        IDeleteChildren<TId>
        where TModel : TModelCreate
    {
    }
}
