using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{

    /// <inheritdoc cref="ICrudDependentToMasterWithUniqueIdBasic{TModelCreate,TModel,TId, TDependentId}" />
    public interface ICrudDependentToMasterWithUniqueIdBasic<TModel, TId, TDependentId> :
        ICrudDependentToMasterBasic<TModel, TId, TDependentId>,
        ICrudDependentToMasterWithUniqueIdBasic<TModel, TModel, TId, TDependentId>
        where TModel : IUniquelyIdentifiable<TId>
    {
    }


    /// <summary>
    /// Functionality for persisting objects that are "dependents" to another object, i.e. they don't have a life of their own. For instance, if their master is deleted, so should they.
    /// Example: A order item is a dependent to an order header.
    /// </summary>
    public interface ICrudDependentToMasterWithUniqueIdBasic<in TModelCreate, TModel, TId, TDependentId> :
        ICrudDependentToMasterBasic<TModelCreate, TModel, TId, TDependentId>,
        IRead<TModel, TId>,
        IUpdate<TModel, TId>,
        IDelete<TId> 
        where TModel : TModelCreate
        where TModelCreate : IUniquelyIdentifiable<TId>
    {
    }
}
