namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <inheritdoc cref="ICrudManyToOneBasic{TModelCreate,TModel,TId}" />
    public interface ICrudManyToOneBasic<TModel, TId> :
        ICrudManyToOneBasic<TModel, TModel, TId>,
        ICrudBasic<TModel, TId>
    {
    }

    /// <summary>
    /// Crud operations for objects that have a parent. This means that apart from the CRUD operations,
    /// there are operations for reading and deleting the children of a parent.
    /// </summary>
    /// <typeparam name="TModelCreate"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface ICrudManyToOneBasic<in TModelCreate, TModel, TId> :
        ICrudBasic<TModelCreate, TModel, TId>,
#pragma warning disable 618
        ISlaveToMaster<TModel, TId>
#pragma warning restore 618
        where TModel : TModelCreate
    {
    }
}
