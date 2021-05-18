using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{
    /// <summary>
    /// General class for storing any <see cref="IUniquelyIdentifiable{TId}"/> in memory.
    /// </summary>
    /// <typeparam name="TModel">The type of objects that are returned from persistant storage.</typeparam>
    /// <typeparam name="TId"></typeparam>
    public class MemoryBase<TModel, TId>
    {
        /// <summary>
        /// Needed for providing lock functionality.
        /// </summary>
        protected readonly ConcurrentDictionary<TId, Lock<TId>> _locks = new ConcurrentDictionary<TId, Lock<TId>>();

        /// <summary>
        /// The actual storage of the items.
        /// </summary>
        protected readonly ConcurrentDictionary<TId, TModel> MemoryItems = new ConcurrentDictionary<TId, TModel>();

        /// <summary>
        /// If <paramref name="item"/> implements <see cref="IOptimisticConcurrencyControlByETag"/>
        /// then the old value is read using <paramref name="service"/> and the values are verified to be equal.
        /// The Etag of the item is then set to a new value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <param name="service"></param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns></returns>
        protected virtual async Task<TModel> MaybeVerifyEtagForUpdateAsync(TId id, TModel item, IRead<TModel, TId> service, CancellationToken token = default(CancellationToken))
        {
            var oldItem = await service.ReadAsync(id, token);
            if (!(item is IOptimisticConcurrencyControlByETag etaggable)) return oldItem;
            if (Equals(oldItem, default(TModel))) return oldItem;
            var oldEtag = (oldItem as IOptimisticConcurrencyControlByETag)?.Etag;
            if (oldEtag?.ToLowerInvariant() != etaggable.Etag?.ToLowerInvariant())
                throw new FulcrumConflictException($"The updated item ({item}) had an old ETag value.");

            return oldItem;
        }
        /// <summary>
        /// Return true if an item iwht id <paramref name="id"/> exists
        /// </summary>
        protected bool Exists(TId id)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            return (MemoryItems.ContainsKey(id));
        }

        /// <summary>
        /// Copy an item into a new instance.
        /// </summary>
        /// <exception cref="FulcrumAssertionFailedException"></exception>
        protected static TModel CopyItem(TModel source)
        {
            InternalContract.RequireNotNull(source, nameof(source));
            var itemCopy = StorageHelper.DeepCopy(source);
            if (itemCopy == null)
                throw new FulcrumAssertionFailedException("Could not copy an item.");
            return itemCopy;
        }

        /// <summary>
        /// Get an item from the memory.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="okIfNotExists">If false, we will throw an exception if the id is not found.</param>
        /// <returns></returns>
        /// <exception cref="FulcrumNotFoundException"></exception>
        protected TModel GetMemoryItem(TId id, bool okIfNotExists = false)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            if (!Exists(id))
            {
                if (!okIfNotExists)
                    throw new FulcrumNotFoundException(
                        $"Could not find an item of type {typeof(TModel).Name} with id \"{id}\".");
                return default(TModel);
            }
            var item = MemoryItems[id];
            FulcrumAssert.IsNotNull(item);
            return CopyItem(item);
        }
    }
}
