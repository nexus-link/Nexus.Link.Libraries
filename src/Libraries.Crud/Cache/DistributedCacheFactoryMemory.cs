using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <summary>
    /// A factory for creating new caches.
    /// </summary>
    public class DistributedCacheFactoryMemory : IDistributedCacheFactory
    {
        private readonly ICrud<DistributedCacheMemory, DistributedCacheMemory, string> _storage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storage"></param>
        public DistributedCacheFactoryMemory(ICrud<DistributedCacheMemory, DistributedCacheMemory, string> storage)
        {
            InternalContract.RequireNotNull(storage, nameof(storage));

            _storage = storage;
        }

        /// <inheritdoc />
        public async Task<IDistributedCache> GetOrCreateDistributedCacheAsync(string key)
        {
            var item = await _storage.ReadAsync(key);
            if (item != null) return item;
            var cache = new DistributedCacheMemory();
            await _storage.CreateWithSpecifiedIdAsync(key, cache);
            return cache;
        }
    }
}
