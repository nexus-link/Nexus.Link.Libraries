using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Lock/unlock an item.
    /// </summary>
    /// <typeparam name="TId">The type for the master id.</typeparam>
    /// <typeparam name="TDependentId">The type for the dependent id.</typeparam>
    public interface ITransactionLockDependent<TModel, in TId, in TDependentId> : ICrudableDependent<TId, TDependentId>
    {
        /// <summary>
        /// Claim a lock for the item uniquely identified by <paramref name="masterId"/> and <paramref name="dependentId"/>
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="dependentId">The id for the dependent object.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released when the transaction has completed or been aborted.
        /// </remarks>
        Task ClaimTransactionLockAsync(TId masterId, TDependentId dependentId, CancellationToken token = default);

        /// <summary>
        /// Returns the item uniquely identified by <paramref name="masterId"/> and <paramref name="dependentId"/> from storage and locks it with a transactional lock.
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="dependentId">The id for the dependent object.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released when the transaction has completed or been aborted.
        /// </remarks>
        Task<TModel> ClaimTransactionLockAndReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = default);

    }
}
