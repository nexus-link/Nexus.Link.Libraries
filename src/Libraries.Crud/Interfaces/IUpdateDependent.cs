using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Update an item.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to update in persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the master id.</typeparam>
    /// <typeparam name="TDependentId">The type for the dependent id.</typeparam>
    public interface IUpdateDependent<in TModel, in TId, in TDependentId> : ICrudableDependent<TModel, TId, TDependentId>
    {
        /// <summary>
        /// Updates the item uniquely identified by <paramref name="masterId"/> and <paramref name="dependentId"/> in storage.
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="dependentId">The id for the dependent object.</param>
        /// <param name="item">The new version of the item.</param>
        /// <param name="token">Propagates notification that operations should be canceled</param>
        /// <exception cref="FulcrumNotFoundException">Thrown if the <paramref name="masterId"/> or the <paramref name="dependentId"/> could not be found.</exception>
        /// <exception cref="FulcrumConflictException">Thrown if the <see cref="IOptimisticConcurrencyControlByETag.Etag"/> for <paramref name="item"/> was outdated.</exception>
        Task UpdateAsync(TId masterId, TDependentId dependentId, TModel item, CancellationToken token = default);
    }
}
