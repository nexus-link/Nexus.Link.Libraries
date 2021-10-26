using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Lock/unlock an item.
    /// </summary>
    /// <typeparam name="TId">The type for the master id and the lock id.</typeparam>
    /// <typeparam name="TDependentId">The type for the dependent id.</typeparam>
    public interface IDependentDistributedLock<TId, TDependentId> : ICrudableDependent<TId, TDependentId>
    {
        /// <summary>
        /// Claim a lock for the item uniquely identified by <paramref name="masterId"/> and <paramref name="dependentId"/>
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="dependentId">The id for the dependent object.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A <see cref="Lock{TId}"/> object that proves that the lock has been claimed.</returns>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released after 30 seconds, but please use <see cref="ReleaseDistributedLockAsync"/> to release the lock as soon as you don't need the lock anymore.
        /// </remarks>
        Task<DependentLock<TId, TDependentId>> ClaimDistributedLockAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default);

        /// <summary>
        /// Releases the lock for an item.
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="dependentId">The id for the dependent object.</param>
        /// <param name="lockId">The id of the lock for this item, to prove that you are eligible of unlocking it.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        Task ReleaseDistributedLockAsync(TId masterId, TDependentId dependentId, TId lockId, CancellationToken cancellationToken  = default);
    }
}
