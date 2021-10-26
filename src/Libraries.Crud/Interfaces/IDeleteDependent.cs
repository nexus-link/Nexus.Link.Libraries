using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Delete items./>.
    /// </summary>
    /// <typeparam name="TId">The type for the master id.</typeparam>
    /// <typeparam name="TDependentId">The type for the dependent id.</typeparam>
    public interface IDeleteDependent<in TId, in TDependentId> : ICrudableDependent<TId, TDependentId>
    {
        /// <summary>
        /// Deletes the item uniquely identified by <paramref name="masterId"/> and <paramref name="dependentId"/> from storage.
        /// </summary>
        /// <param name="masterId">The id for the master object.</param>
        /// <param name="dependentId">The id for the dependent object.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        Task DeleteAsync(TId masterId, TDependentId dependentId, CancellationToken cancellationToken  = default);
    }
}
