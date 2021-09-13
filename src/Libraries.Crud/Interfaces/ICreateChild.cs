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
    public interface ICreateChild<in TModel, TId> : ICreateChild<TModel, TModel, TId>
    {
    }

    /// <summary>
    /// Can create items."/>.
    /// </summary>
    /// <typeparam name="TModelCreate">The type for creating objects in persistent storage.</typeparam>
    /// <typeparam name="TModel">The type of objects that are returned from persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the <see cref="IUniquelyIdentifiable{TId}.Id"/> property.</typeparam>
    public interface ICreateChild<in TModelCreate, in TModel, TId> : ICrudable<TModel, TId>
    where TModel : TModelCreate
    {
        /// <summary>
        /// Creates a new item in storage and returns the new Id.
        /// </summary>
        /// <param name="parentId">The specific parent to create a child item for.</param>
        /// <param name="item">The item to store.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>The new id for the created child.</returns>
        Task<TId> CreateChildAsync(TId parentId, TModelCreate item, CancellationToken token = default);
    }
}
