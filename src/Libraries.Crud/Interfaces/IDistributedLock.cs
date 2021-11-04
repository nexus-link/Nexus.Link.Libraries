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
    /// <typeparam name="TId">The type for the object id and the lock id.</typeparam>
    public interface IDistributedLock<TId> : ICrudable<TId>
    {
        /// <summary>
        /// Claim a lock for the item with id <paramref name="id"/>
        /// </summary>
        /// <param name="id">The id for the object to be locked.</param>
        /// <param name="lockTimeSpan">After this time, the lock will be released automatically. Null means that the implementation can choose a time span.</param>
        /// <param name="currentLockId">If you currently have a lock and would like to extend it, pass the old lock id here.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A <see cref="Lock{TLockId, TObjectId}"/> object that proves that the lock has been claimed.</returns>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released after 30 seconds, but please use <see cref="ReleaseDistributedLockAsync"/> to release the lock as soon as you don't need the lock anymore.
        /// </remarks>
        Task<Lock<TId>> ClaimDistributedLockAsync(TId id, TimeSpan? lockTimeSpan = null, TId currentLockId = default, CancellationToken cancellationToken  = default);

        /// <summary>
        /// Releases the lock for an object.
        /// </summary>
        /// <param name="id">The id of the item that should be released.</param>
        /// <param name="lockId">The id of the lock for this item, to prove that you are eligible of unlocking it.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken cancellationToken  = default);
    }

    /// <summary>
    /// Lock/unlock an item.
    /// </summary>
    /// <typeparam name="TObjectId">The type for the object id .</typeparam>
    /// <typeparam name="TLockId">The type for the lock id.</typeparam>
    public interface IDistributedLock<TObjectId, TLockId> : ICrudable<TObjectId>
    {
        /// <summary>
        /// Claim a lock for the item with id <paramref name="id"/>
        /// </summary>
        /// <param name="id">The id for the object to be locked.</param>
        /// <param name="lockTimeSpan">After this time, the lock will be released automatically. Null means that the implementation can choose a time span.</param>
        /// <param name="currentLockId">If you currently have a lock and would like to extend it, pass the old lock id here.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A <see cref="Lock{TLockId, TObjectId}"/> object that proves that the lock has been claimed.</returns>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released after 30 seconds, but please use <see cref="ReleaseDistributedLockAsync"/> to release the lock as soon as you don't need the lock anymore.
        /// </remarks>
        Task<Lock<TObjectId, TLockId>> ClaimDistributedLockAsync(TObjectId id, TimeSpan? lockTimeSpan = null, TLockId currentLockId = default, CancellationToken cancellationToken  = default);

        /// <summary>
        /// Releases the lock for an object.
        /// </summary>
        /// <param name="id">The id of the item that should be released.</param>
        /// <param name="lockId">The id of the lock for this item, to prove that you are eligible of unlocking it.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        Task ReleaseDistributedLockAsync(TObjectId id, TLockId lockId, CancellationToken cancellationToken  = default);
    }
}
