using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <summary>
    /// An interface for producing caches with interface <see cref="IDistributedCache"/>
    /// </summary>
    public interface IDistributedCacheFactory
    {
        /// <summary>
        /// Create or get a distributed cache.
        /// </summary>
        /// <param name="key">The key to find the cache if it already exists.</param>
        /// <param name="cancellationToken"></param>
        Task<IDistributedCache> GetOrCreateDistributedCacheAsync(string key, CancellationToken cancellationToken = default);
    }
}
