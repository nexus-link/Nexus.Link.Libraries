using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Delete items./>.
    /// </summary>
    public interface IDeleteSlave<in TId> : ICrudable<TId>
    {
        /// <summary>
        /// Deletes the item uniquely identified by <paramref name="masterId"/> and <paramref name="slaveId"/> from storage.
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="slaveId">The id for the slave object.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        Task DeleteAsync(TId masterId, TId slaveId, CancellationToken token = default);
    }
}
