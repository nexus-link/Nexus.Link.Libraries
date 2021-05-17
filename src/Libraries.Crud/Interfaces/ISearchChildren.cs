using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Read items"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to search for in persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id of the object.</typeparam>
    public interface ISearchChildren<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Search for matching items and return them as pages, with the items ordered as specified.
        /// </summary>
        /// <param name="parentId">The specific parent to search the child items for.</param>
        /// <param name="details">The search details</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A page of the found items.</returns>
        /// <remarks>
        /// The implementor of this method can decide that it is not a valid method to expose.
        /// In that case, the method should throw a <see cref="FulcrumNotImplementedException"/>.
        /// </remarks>
        Task<PageEnvelope<TModel>> SearchChildrenAsync(TId parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Search for the first matching item.
        /// </summary>
        /// <param name="parentId">The specific parent to search the child items for.</param>
        /// <param name="details">The search details</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A page of the found items.</returns>
        /// <remarks>
        /// The implementor of this method can decide that it is not a valid method to expose.
        /// In that case, the method should throw a <see cref="FulcrumNotImplementedException"/>.
        /// </remarks>
        Task<TModel> SearchFirstChildAsync(TId parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Search for a unique matching item.
        /// </summary>
        /// <param name="parentId">The specific parent to search the child items for.</param>
        /// <param name="details">The search details</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>The found item or null.</returns>
        /// <remarks>
        /// The implementor of this method can decide that it is not a valid method to expose.
        /// In that case, the method should throw a <see cref="FulcrumNotImplementedException"/>.
        /// </remarks>
        Task<TModel> FindUniqueChildAsync(TId parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
