using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.MemoryStorage;

namespace Nexus.Link.Libraries.Crud.Cache
{
    /// <summary>
    /// 
    /// </summary>
    public class DistributedCacheMemory : IDistributedCache, IFlushableCache
    {
        /// <summary>
        /// The actual storage of the items.
        /// </summary>
        protected readonly CrudMemory<byte[], byte[], string> ItemStorage = new CrudMemory<byte[], byte[], string>();


        /// <inheritdoc />
        public byte[] Get(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<byte[]> GetAsync(string key, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            return await ItemStorage.ReadAsync(key, cancellationToken );
        }

        /// <inheritdoc />
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            InternalContract.RequireNotNull(value, nameof(value));

            var item = await GetAsync(key, cancellationToken );
            if (item == null)
            {
                try
                {
                    await ItemStorage.CreateWithSpecifiedIdAsync(key, value, cancellationToken );
                }
                catch (FulcrumConflictException)
                {
                    await ItemStorage.UpdateAsync(key, value, cancellationToken );
                }
            }
            else
            {
                await ItemStorage.UpdateAsync(key, value, cancellationToken );
            }
        }

        /// <inheritdoc />
        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task RefreshAsync(string key, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken cancellationToken  = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(key, nameof(key));
            await ItemStorage.DeleteAsync(key, cancellationToken );
        }

        /// <inheritdoc />
        public async Task FlushAsync(CancellationToken cancellationToken  = default)
        {
            await ItemStorage.DeleteAllAsync(cancellationToken );
        }
    }
}

