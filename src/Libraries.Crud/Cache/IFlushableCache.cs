using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <summary>
    /// Interface for a flush method for the cache. The flush method should reset the cache to empty.
    /// </summary>
    public interface IFlushableCache
    {
        /// <summary>
        /// Clears the cache, i.e. remove all cached items.
        /// </summary>
        /// <param name="cancellationToken ">Propagates notification that operations should be canceled.</param>
        Task FlushAsync(CancellationToken cancellationToken  = default);
    }
}
