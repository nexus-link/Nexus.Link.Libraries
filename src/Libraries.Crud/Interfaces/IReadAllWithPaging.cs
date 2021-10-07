using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Read items"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to read from persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id of the object.</typeparam>
    public interface IReadAllWithPaging<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Reads all the items from storage and return them as pages.
        /// </summary>
        /// <returns>A page of the found items.</returns>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="cancellationToken ">Propagates notification that operations should be canceled</param>
        /// <remarks>
        /// The implementor of this method can decide that it is not a valid method to expose.
        /// In that case, the method should throw a <see cref="FulcrumNotImplementedException"/>.
        /// </remarks>
        Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken  = default);
    }
}
