using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Delete items./>.
    /// </summary>
    [Obsolete("Use IDeleteDependent. Obsolete since 2021-08-27.")]
    public interface IDeleteSlave<in TId> : ICrudable<TId>
    {
        /// <summary>
        /// Deletes the item uniquely identified by <paramref name="masterId"/> and <paramref name="slaveId"/> from storage.
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="slaveId">The id for the slave object.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        Task DeleteAsync(TId masterId, TId slaveId, CancellationToken cancellationToken  = default);
    }
}
