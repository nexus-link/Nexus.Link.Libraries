using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Can create items."/>.
    /// </summary>
    /// <typeparam name="TModel">The type for creating objects in persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id of the stored objects.</typeparam>
    public interface ICreateWithSpecifiedId<TModel, in TId> : ICreateWithSpecifiedId<TModel, TModel, TId>, ICreateWithSpecifiedIdNoReturn<TModel, TId>, ICreateWithSpecifiedIdAndReturn<TModel, TId>
    {
    }

    /// <summary>
    /// Can create items."/>.
    /// </summary>
    /// <typeparam name="TModelCreate">The type for creating objects in persistent storage.</typeparam>
    /// <typeparam name="TModel">The type of objects that are returned from persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the <see cref="IUniquelyIdentifiable{TId}.Id"/> property.</typeparam>
    public interface ICreateWithSpecifiedId<in TModelCreate, TModel, in TId> : ICreateWithSpecifiedIdNoReturn<TModelCreate, TModel, TId>, ICreateWithSpecifiedIdAndReturn<TModelCreate, TModel, TId>
    where TModel : TModelCreate
    {
    }
}
