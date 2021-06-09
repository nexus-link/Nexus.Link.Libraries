using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Lock/unlock an item.
    /// </summary>
    /// <typeparam name="TId">The type for the id parameter.</typeparam>
    [Obsolete("Use IDistributedLock. Obsolete warning since 2021-04-29")]
    public interface ILockable<TId> : ICrudable<TId>
    {
        /// <summary>
        /// Claim a lock for the item with id <paramref name="id"/>
        /// </summary>
        /// <param name="id">How the object to be locked is identified.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <returns>A <see cref="Lock{TId}"/> object that proves that the lock has been claimed.</returns>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there alread is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released after 30 seconds, but please use <see cref="ReleaseLockAsync"/> to release the lock as soon as you don't need the lock anymore.
        /// </remarks>
        Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = default);

        /// <summary>
        /// Releases the lock for an object.
        /// </summary>
        /// <param name="id">The id of the item that should be release.</param>
        /// <param name="lockId">The id of the lock for this item, to prove that you are eligable of unlocking it.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task ReleaseLockAsync(TId id, TId lockId, CancellationToken token = default);
    }
}
