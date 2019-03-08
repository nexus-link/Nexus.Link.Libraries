namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <inheritdoc cref="ICrud{TModelCreate, TModel,TId}" />
    public interface ICrudBasic<TModel, TId> :
        ICrudBasic<TModel, TModel, TId>,
        ICreate<TModel, TId>
    {
    }

    /// <summary>
    /// Interface for CRUD operations."/>.
    /// </summary>
    public interface ICrudBasic<in TModelCreate, TModel, TId> :
        ICreate<TModelCreate, TModel, TId>,
        IRead<TModel, TId>,
        IReadAllWithPaging<TModel, TId>,
        IUpdate<TModel, TId>,
        IDelete<TId>,
        IDeleteAll
        where TModel : TModelCreate
    {
    }
}
