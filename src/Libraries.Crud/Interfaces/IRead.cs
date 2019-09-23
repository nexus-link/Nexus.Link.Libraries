using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Read items"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to read from persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id of the object.</typeparam>
    public interface IRead<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Returns the item uniquely identified by <paramref name="id"/> from storage.
        /// </summary>
        /// <returns>The found item or null.</returns>
        Task<TModel> ReadAsync(TId id, CancellationToken token = default(CancellationToken));
    }
}
