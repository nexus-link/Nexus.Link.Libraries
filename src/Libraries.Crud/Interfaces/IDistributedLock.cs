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
    public interface IDistributedLock<TId> : IDistributedLock<TId, TId>
    {
    }

    /// <summary>
    /// Lock/unlock an item.
    /// </summary>
    /// <typeparam name="TObjectId">The type for the object id .</typeparam>
    /// <typeparam name="TLockId">The type for the lock id.</typeparam>
    public interface IDistributedLock<TLockId, in TObjectId> : ICrudable<TObjectId>
    {
        /// <summary>
        /// Claim a lock for the item with id <paramref name="id"/>
        /// </summary>
        /// <param name="id">How the object to be locked is identified.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A <see cref="Lock{TId}"/> object that proves that the lock has been claimed.</returns>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released after 30 seconds, but please use <see cref="ReleaseDistributedLockAsync"/> to release the lock as soon as you don't need the lock anymore.
        /// </remarks>
        Task<Lock<TLockId>> ClaimDistributedLockAsync(TObjectId id, CancellationToken cancellationToken  = default);

        /// <summary>
        /// Releases the lock for an object.
        /// </summary>
        /// <param name="objectId">The id of the item that should be release.</param>
        /// <param name="lockId">The id of the lock for this item, to prove that you are eligable of unlocking it.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        Task ReleaseDistributedLockAsync(TObjectId objectId, TLockId lockId, CancellationToken cancellationToken  = default);
    }
}
