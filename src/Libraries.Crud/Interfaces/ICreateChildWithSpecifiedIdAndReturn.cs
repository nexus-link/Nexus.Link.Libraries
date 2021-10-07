using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Can create child items."/>.
    /// </summary>
    /// <typeparam name="TModel">The type for creating objects in persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id of the stored objects.</typeparam>
    public interface ICreateChildWithSpecifiedIdAndReturn<TModel, in TId> : ICreateChildWithSpecifiedIdAndReturn<TModel, TModel, TId>
    {
    }

    /// <summary>
    /// Functionality for persisting objects that has no life of their own, but are only relevant with their parent.
    /// Examples: A list of rows on an invoice, a list of attributes of an object, the contact details of a person.
    /// </summary>
    public interface ICreateChildWithSpecifiedIdAndReturn<in TModelCreate, TModel, in TId> : ICrudable<TModel, TId>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Same as <see cref="ICreateChildAndReturn{TModelCreate,TModel,TId}.CreateChildAndReturnAsync"/>, but you can specify the new id.
        /// </summary>
        /// <param name="parentId">The id of the parent for this child.</param>
        /// <param name="childId">The proposed id for this child.</param>
        /// <param name="item">The item to store.</param>
        /// <param name="cancellationToken ">Propagates notification that operations should be canceled</param>
        /// <returns>The new item as it was saved, see remarks below.</returns>
        /// <remarks>
        /// If the returned type implements <see cref="IUniquelyIdentifiable{TChildId}"/>, then the <see cref="IUniquelyIdentifiable{TChildId}.Id"/> is updated with the new id. 
        /// If it implements <see cref="IOptimisticConcurrencyControlByETag"/>, then the <see cref="IOptimisticConcurrencyControlByETag.Etag"/> is updated..
        /// </remarks>
        /// <seealso cref="IOptimisticConcurrencyControlByETag"/>
        /// <seealso cref="IUniquelyIdentifiable{TId}"/>
        Task<TModel> CreateChildWithSpecifiedIdAndReturnAsync(TId parentId, TId childId, TModelCreate item, CancellationToken cancellationToken  = default);
    }
}
