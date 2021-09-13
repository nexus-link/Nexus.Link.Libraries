using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Read items"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to read from persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the master id.</typeparam>
    /// <typeparam name="TDependentId">The type for the dependent id.</typeparam>
    public interface IReadDependent<TModel, in TId, in TDependentId> : ICrudableDependent<TModel, TId, TDependentId>
    {
        /// <summary>
        /// Returns the item uniquely identified by <paramref name="masterId"/> and <paramref name="dependentId"/> from storage.
        /// </summary>
        /// <returns>The found item or null.</returns>
        Task<TModel> ReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = default);
    }
}
