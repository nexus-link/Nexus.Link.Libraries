using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.SqlServer.Model
{
    public interface IDistributedLockTable : ICreateAndReturn<DistributedLockRecord, Guid>, IUpdateAndReturn<DistributedLockRecord, Guid>, IDelete<Guid>
    {
        /// <summary>
        /// Try to add a lock record.
        /// </summary>
        /// <param name="recordToLockId">The id of the object that should be locked.</param>
        /// <param name="lockTimeSpan">The time span that the lock should be valid.</param>
        /// <param name="currentLockId">If we are prolonging a lock, this is the current lock id.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Information about the lock</returns>
        /// <exception cref="FulcrumTryAgainException">
        /// Thrown if there already is a claimed lock. Will contain information about when the lock is automatically released.
        /// </exception>
        Task<Lock<Guid>> TryAddAsync(Guid recordToLockId, TimeSpan? lockTimeSpan = null, Guid? currentLockId = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Release a lock.
        /// </summary>
        /// <param name="lockedRecordId">The id of the object that has a lock.</param>
        /// <param name="lockId">The current lock id.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RemoveAsync(Guid lockedRecordId, Guid lockId, CancellationToken cancellationToken = default);
    }
}