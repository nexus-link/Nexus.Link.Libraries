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
    /// <typeparam name="TModel">THe type for the data returned.</typeparam>
    public interface ITransactionLock<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Claim a lock for the item with id <paramref name="id"/>
        /// </summary>
        /// <param name="id">How the object to be locked is identified.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released when the transaction has completed or been aborted.
        /// </remarks>
        Task ClaimTransactionLockAsync(TId id, CancellationToken token = default(CancellationToken));

        
        /// <summary>
        /// Returns the item uniquely identified by <paramref name="id"/> from storage and locks it with a transactional lock.
        /// Same as first calling <see cref="ClaimTransactionLockAsync"/> and then <see cref="IRead{TModel,TId}.ReadAsync"/>.
        /// </summary>
        /// <returns>The found item or null.</returns>
        /// <param name="id">How the object to be locked is identified.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock.
        /// </exception>
        /// <remarks>
        /// The lock will be automatically released when the transaction has completed or been aborted.
        /// </remarks>
        Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken token = default(CancellationToken));
    }
}
