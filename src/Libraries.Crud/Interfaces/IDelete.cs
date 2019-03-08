using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Delete items./>.
    /// </summary>
    public interface IDelete<in TId> : ICrudable<TId>
    {
        /// <summary>
        /// Deletes the item uniquely identified by <paramref name="id"/> from storage.
        /// </summary>
        /// <param name="id">The id of the item that should be deleted.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task DeleteAsync(TId id, CancellationToken token = default(CancellationToken));
    }
}
