using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Read items"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to read from persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id of the object.</typeparam>
    [Obsolete("Use IReadDependent. Obsolete since 2021-08-27.")]
    public interface IReadSlave<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Returns the item uniquely identified by <paramref name="masterId"/> and <paramref name="slaveId"/> from storage.
        /// </summary>
        /// <returns>The found item or null.</returns>
        Task<TModel> ReadAsync(TId masterId, TId slaveId, CancellationToken cancellationToken  = default);
    }
}
