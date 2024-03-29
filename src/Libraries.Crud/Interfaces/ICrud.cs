﻿// ReSharper disable RedundantExtendsListEntry
namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <inheritdoc cref="ICrud{TModelCreate, TModel,TId}" />
    public interface ICrud<TModel, TId> : 
        ICrud<TModel, TModel, TId>,
        ICreate<TModel, TId>,
        ICreateAndReturn<TModel, TId>,
        ICreateWithSpecifiedId<TModel, TId>,
        ICrudBasic<TModel, TId>
    {
    }

    /// <summary>
    /// Interface for CRUD operations."/>.
    /// </summary>
    public interface ICrud<in TModelCreate, TModel, TId> :
        ICreateAndReturn<TModelCreate, TModel, TId>,
        ICreateWithSpecifiedId<TModelCreate, TModel, TId>,
        IReadAll<TModel, TId>,
        ISearch<TModel, TId>,
        IUpdateAndReturn<TModel, TId>,
#pragma warning disable 618
        ILockable<TId>,
#pragma warning restore 618
        IDistributedLock<TId>,
        ITransactionLock<TModel, TId>,
        ICrudBasic<TModelCreate, TModel, TId> 
        where TModel : TModelCreate
    {
    }
}
