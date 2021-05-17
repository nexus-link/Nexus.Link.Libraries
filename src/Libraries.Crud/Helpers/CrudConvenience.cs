using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.Helpers
{
    public class CrudConvenience<TModelCreate, TModel, TId> : ICreateAndReturn<TModelCreate, TModel, TId>, IReadAll<TModel, TId>, ISearch<TModel, TId>
        where TModel : TModelCreate
    {
        private readonly ICrud<TModelCreate, TModel, TId> _service;

        public CrudConvenience(ICrudable<TModel, TId> service)
        {
            _service = new CrudPassThrough<TModelCreate,TModel, TId>(service);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(item, nameof(item));
            var id = await _service.CreateAsync(item, token);
            return await _service.ReadAsync(id, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TModel>> ReadAllAsync(int limit = Int32.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return StorageHelper.ReadPagesAsync<TModel>((offset, ct) => _service.ReadAllWithPagingAsync(offset, null, ct), limit, token);
        }

        /// <summary>
        /// This method will get all items and then do the filter and sorting in memory.
        /// </summary>
        /// <param name="details">The search details</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A page of the found items.</returns>
        /// <remarks>If your persistence layer supports search, then avoid using this, at it always reads all the items.</remarks>
        public async Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            var allItems = (await _service.ReadAllAsync(int.MaxValue, cancellationToken))
                .ToList();

            var list = SearchHelper<TModel>.FilterAndSort(allItems, details)
                .Skip(offset)
                .Take(limit.Value);
            var page = new PageEnvelope<TModel>(offset, limit.Value, allItems.Count(), list);
            return page;
        }

        /// <inheritdoc />
        public async Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            var page = await _service.SearchAsync(details, 0, 2, cancellationToken);
            if (page.PageInfo.Returned > 1)
            {
                throw new FulcrumContractException($"Expected to find unique value, but found multiple items for search for model {nameof(TModel)} with {nameof(details)}:\r{details}");
            }

            return page.Data.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<TModel> SearchFirstAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            var page = await _service.SearchAsync(details, 0, 1, cancellationToken);
            return page.Data.FirstOrDefault();
        }
    }
}