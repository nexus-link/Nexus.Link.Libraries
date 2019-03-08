using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Functionality for persisting objects that references a parent object in another table.
    /// </summary>
    public interface IReadChildrenWithPaging<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Read all child items for a specific parent, <paramref name="parentId"/>.
        /// </summary>
        /// <param name="parentId">The specific parent to read the child items for.</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId parentId, int offset, int? limit = null, CancellationToken token = default(CancellationToken));
    }
}
