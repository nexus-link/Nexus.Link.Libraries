using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Delete the children of a parent.
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public interface IDeleteChildren<in TId> : ICrudable<TId>
    {
        /// <summary>
        /// Delete all child items for a specific parent, <paramref name="parentId"/>.
        /// </summary>
        Task DeleteChildrenAsync(TId parentId, CancellationToken cancellationToken  = default);
    }
}
