
// ReSharper disable RedundantExtendsListEntry

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting many-to-many relations.
    /// </summary>
    /// <typeparam name="TManyToManyModel">The type of objects that are returned from persistent storage.</typeparam>
    /// <typeparam name="TReferenceModel1"></typeparam>
    /// <typeparam name="TReferenceModel2"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface ICrudManyToMany<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId> :
        ICrudManyToMany<TManyToManyModel, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>,
        ICrud<TManyToManyModel, TId>,
        ICreateSlaveWithSpecifiedId<TManyToManyModel, TId>,
        IManyToMany<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>,
        ICrudManyToManyBasic<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting many-to-many relations.
    /// </summary>
    public interface ICrudManyToMany<in TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId> :
        ICrudManyToManyBasic<TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>,
        ICrud<TManyToManyModelCreate, TManyToManyModel, TId>,
        ICreateSlaveWithSpecifiedId<TManyToManyModelCreate, TManyToManyModel, TId>,
        IReadSlave<TManyToManyModel, TId>,
        IUpdateSlave<TManyToManyModel, TId>,
        IUpdateSlaveAndReturn<TManyToManyModel, TId>,
        IDeleteSlave<TId>,
        IManyToMany<TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
        where TManyToManyModel : TManyToManyModelCreate
    {
    }
}
