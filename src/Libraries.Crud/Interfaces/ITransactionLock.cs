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
    public interface ITransactionLock<in TId> : ICrudable<TId>
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
    }
}
