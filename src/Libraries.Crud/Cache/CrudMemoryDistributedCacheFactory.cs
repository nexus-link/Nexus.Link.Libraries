using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <summary>
    /// A factory for creating new caches.
    /// </summary>
    public class CrudMemoryDistributedCacheFactory : IDistributedCacheFactory
    {
        private readonly ICrud<CrudMemoryDistributedCache, CrudMemoryDistributedCache, string> _storage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage"></param>
        public CrudMemoryDistributedCacheFactory(ICrud<CrudMemoryDistributedCache, CrudMemoryDistributedCache, string> storage)
        {
            InternalContract.RequireNotNull(storage, nameof(storage));
            _storage = storage;
        }

        /// <inheritdoc />
        public async Task<IDistributedCache> GetOrCreateDistributedCacheAsync(string key, CancellationToken cancellationToken = default)
        {
            var cache = await _storage.ReadAsync(key, cancellationToken);

            if (cache != null) return cache;
            cache = new CrudMemoryDistributedCache();
            await _storage.CreateWithSpecifiedIdAsync(key, cache, cancellationToken);
            return cache;
        }
    }
}
