using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.EntityAttributes;

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
        [CrudHint.CentralMethod("read", CrudMethodEnum.ReadAsync, "E266ADF1-E224-4F26-B0D1-3622A1146B19")]
        Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken  = default);
    }
}
