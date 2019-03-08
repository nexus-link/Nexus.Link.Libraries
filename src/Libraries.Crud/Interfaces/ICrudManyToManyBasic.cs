namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <inheritdoc cref="ICrudManyToManyBasic{TManyToManyModelCreate, TManyToManyModel,TReferenceModel1,TReferenceModel2,TId}" />
    public interface ICrudManyToManyBasic<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId> :
        ICrudManyToManyBasic<TManyToManyModel, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>,
        ICrudBasic<TManyToManyModel, TId>,
        IManyToMany<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting many-to-many relations.
    /// </summary>
    public interface ICrudManyToManyBasic<in TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId> :
        ICrudBasic<TManyToManyModelCreate, TManyToManyModel, TId>,
        IReadSlave<TManyToManyModel, TId>,
        IUpdateSlave<TManyToManyModel, TId>,
        IDeleteSlave<TId>,
        IManyToMany<TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
        where TManyToManyModel : TManyToManyModelCreate
    {
    }
}
