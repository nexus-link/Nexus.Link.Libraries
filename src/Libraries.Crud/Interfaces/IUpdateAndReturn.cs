﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Update an item.
    /// </summary>
    /// <typeparam name="TModel">The type of objects to update in persistent storage.</typeparam>
    /// <typeparam name="TId">The type for the id parameter.</typeparam>
    public interface IUpdateAndReturn<TModel, in TId> : ICrudable<TModel, TId>
    {
        /// <summary>
        /// Updates the item uniquely identified by <paramref name="item.Id"/> in storage.
        /// </summary>
        /// <param name="id">How the object to be updated is identified.</param>
        /// <param name="item">The new version of the item. </param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>The updated item as it was saved.</returns>
        /// <exception cref="FulcrumNotFoundException">Thrown if the <paramref name="id"/> could not be found.</exception>
        /// <exception cref="FulcrumConflictException">Thrown if the <see cref="IOptimisticConcurrencyControlByETag.Etag"/> for <paramref name="item"/> was outdated.</exception>
        Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken  = default);
    }
}
