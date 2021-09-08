// ReSharper disable RedundantExtendsListEntry
namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <inheritdoc cref="ICrudManyToOne{TModelCreate,TModel,TId}" />
    public interface ICrudManyToOne<TModel, TId> : 
        ICrudManyToOne<TModel, TModel, TId>,
        ICrud<TModel, TId>,
        ICrudManyToOneBasic<TModel, TId>,
        ICreateChild<TModel, TId>,
        ICreateChildAndReturn<TModel, TId>,
        ICreateChildWithSpecifiedId<TModel, TId>,
        ICreateChildWithSpecifiedIdAndReturn<TModel, TId>
    {
    }

    /// <summary>
    /// Crud operations for objects that have a parent. This means that apart from the CRUD operations,
    /// there are operations for reading and deleting the children of a parent.
    /// </summary>
    /// <typeparam name="TModelCreate"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface ICrudManyToOne<in TModelCreate, TModel, TId> :
        ICrud<TModelCreate, TModel, TId>,
#pragma warning disable 618
        ISlaveToMaster<TModel, TId>,
#pragma warning restore 618
        ICreateChild<TModelCreate, TModel, TId>,
        ICreateChildAndReturn<TModelCreate, TModel, TId>,
        ICreateChildWithSpecifiedId<TModelCreate, TModel, TId>,
        ICreateChildWithSpecifiedIdAndReturn<TModelCreate, TModel, TId>,
        IReadChildren<TModel, TId>,
        IReadChildrenWithPaging<TModel, TId>,
        ISearchChildren<TModel, TId>,
        ICrudManyToOneBasic<TModelCreate, TModel, TId>
        where TModel : TModelCreate
    {
    }
}
