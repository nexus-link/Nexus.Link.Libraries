using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Read items"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to search for in persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id of the object.</typeparam>
    public interface ISearch<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Search for matching items and return them as pages in no specific item order.
        /// </summary>
        /// <param name="condition">The properties with values to search for. </param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A page of the found items.</returns>
        /// <remarks>
        /// The implementor of this method can decide that it is not a valid method to expose.
        /// In that case, the method should throw a <see cref="FulcrumNotImplementedException"/>.
        /// </remarks>
        Task<PageEnvelope<TModel>> SearchAsync(object condition, int offset, int? limit = null, CancellationToken cancellationToken = default(CancellationToken));

        ///// <summary>
        ///// Search for matching items and return them as pages, with the items ordered as specified.
        ///// </summary>
        ///// <param name="condition">The field </param>
        ///// <param name="order">The fields that we should order by with a boolean value where true means ascending and false means descending.</param>
        ///// <param name="offset">The number of items that will be skipped in result.</param>
        ///// <param name="limit">The maximum number of items to return.</param>
        ///// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        ///// <returns>A page of the found items.</returns>
        ///// <remarks>
        ///// The implementor of this method can decide that it is not a valid method to expose.
        ///// In that case, the method should throw a <see cref="FulcrumNotImplementedException"/>.
        ///// </remarks>
        Task<PageEnvelope<TModel>> SearchOrderByAsync(object condition, object order, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
