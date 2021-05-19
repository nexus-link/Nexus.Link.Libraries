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
    public interface ITransactionLockSlave<TModel, in TId> : ICrudable<TId>
    {
        /// <summary>
        /// Claim a lock for the item uniquely identified by <paramref name="masterId"/> and <paramref name="slaveId"/>
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="slaveId">The id for the slave object.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released when the transaction has completed or been aborted.
        /// </remarks>
        Task ClaimTransactionLockAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Returns the item uniquely identified by <paramref name="masterId"/> and <paramref name="id"/> from storage and locks it with a transactional lock.
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="slaveId">The id for the slave object.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released when the transaction has completed or been aborted.
        /// </remarks>
        Task<TModel> ClaimTransactionLockAndReadAsync(TId masterId, TId slaveId, CancellationToken token = default(CancellationToken));

    }
}
