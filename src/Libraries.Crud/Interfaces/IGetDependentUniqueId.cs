using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Read items"/>.
    /// </summary>
    /// <typeparam name="TId">The type for the master id.</typeparam>
    /// <typeparam name="TDependentId">The type for the dependent id.</typeparam>
    public interface IGetDependentUniqueId<TId, in TDependentId> : ICrudableDependent<TId, TDependentId>
    {
        /// <summary>
        /// Returns the unique id for the item identified by <paramref name="masterId"/> and <paramref name="dependentId"/>.
        /// </summary>
        /// <returns>The found id or exception.</returns>
        /// <exception cref="FulcrumNotFoundException"></exception>
        Task<TId> GetDependentUniqueIdAsync(TId masterId, TDependentId dependentId, CancellationToken token = default);
    }
}
