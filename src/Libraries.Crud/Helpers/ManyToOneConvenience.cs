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
    public class ManyToOneConvenience<TModelCreate, TModel, TId> : CrudConvenience<TModelCreate, TModel, TId>, ISearchChildren<TModel, TId>
        where TModel : TModelCreate
    {
        private readonly ICrudManyToOne<TModelCreate, TModel, TId> _service;

        public ManyToOneConvenience(ICrudable<TModel, TId> service) : base(service)
        {
            _service = new ManyToOnePassThrough<TModelCreate, TModel, TId>(service);
        }

        /// <summary>
        /// This method will get all items and then do the filter and sorting in memory.
        /// </summary>
        /// <param name="parentId">The specific parent to search the child items for.</param>
        /// <param name="details">The search details</param>
        /// <param name="offset">The number of items that will be skipped in result.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>A page of the found items.</returns>
        /// <remarks>If your persistence layer supports search, then avoid using this, at it always reads all the items.</remarks>
        public async Task<PageEnvelope<TModel>> SearchChildrenAsync(TId parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));

            var allItems = (await _service.ReadChildrenAsync(parentId, int.MaxValue, cancellationToken))
                .ToList();

            var list = SearchHelper.FilterAndSort(allItems, details)
                .Skip(offset)
                .Take(limit.Value);
            var page = new PageEnvelope<TModel>(offset, limit.Value, allItems.Count(), list);
            return page;
        }

        /// <inheritdoc />
        public async Task<TModel> FindUniqueChildAsync(TId parentId, SearchDetails<TModel> details, CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            var page = await _service.SearchChildrenAsync(parentId, details, 0, 2, cancellationToken);
            if (page.PageInfo.Returned > 1)
            {
                throw new FulcrumContractException($"Expected to find unique value, but found multiple items for search for model {nameof(TModel)} with {nameof(details)}:\r{details}");
            }

            return page.Data.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<TModel> SearchFirstChildAsync(TId parentId, SearchDetails<TModel> details, CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.RequireNotNull(details, nameof(details));
            InternalContract.RequireValidated(details, nameof(details));
            var page = await _service.SearchChildrenAsync(parentId, details, 0, 1, cancellationToken);
            return page.Data.FirstOrDefault();
        }
    }
}