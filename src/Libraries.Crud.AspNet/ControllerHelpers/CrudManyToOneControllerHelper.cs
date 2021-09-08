using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http;
#endif
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.AspNet.ControllerHelpers
{
    /// <inheritdoc cref="CrudManyToOneControllerHelper{TModel, TModel}" />
    public class CrudManyToOneControllerHelper<TModel> :
        CrudManyToOneControllerHelper<TModel, TModel>,
        ICrudManyToOne<TModel, string>
    {
        /// <inheritdoc />
        public CrudManyToOneControllerHelper(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <inheritdoc cref="CrudControllerHelper{TModelCreate,TModel}" />
    public class CrudManyToOneControllerHelper<TModelCreate, TModel> :
        CrudControllerHelper<TModelCreate, TModel>,
        ICrudManyToOne<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {

        private readonly ManyToOneConvenience<TModelCreate, TModel, string> _convenience;
        /// <summary>
        /// The logic to be used
        /// </summary>
        protected new readonly ICrudManyToOne<TModelCreate, TModel, string> Logic;

        /// <inheritdoc />
        public CrudManyToOneControllerHelper(ICrudable<TModel, string> logic)
        : base(logic)
        {
            Logic = new ManyToOnePassThrough<TModelCreate, TModel, string>(logic);
            _convenience = new ManyToOneConvenience<TModelCreate, TModel, string>(this);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
            CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                ServiceContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }

            var page = await Logic.ReadChildrenWithPagingAsync(parentId, offset, limit, token);
            FulcrumAssert.IsNotNull(page?.Data);
            FulcrumAssert.IsValidated(page?.Data);
            return page;
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(string parentId, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            await Logic.DeleteChildrenAsync(parentId, token);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = 2147483647, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            ServiceContract.RequireGreaterThan(0, limit, nameof(limit));
            var items = await Logic.ReadChildrenAsync(parentId, limit, token);
            FulcrumAssert.IsNotNull(items);
            FulcrumAssert.IsValidated(items);
            return items;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> SearchChildrenAsync(string parentId, [FromBody] SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                ServiceContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }

            var page = await Logic.SearchChildrenAsync(parentId, details, offset, limit, cancellationToken);
            FulcrumAssert.IsNotNull(page?.Data);
            FulcrumAssert.IsValidated(page?.Data);
            return page;
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(string parentId, [FromBody] SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> CreateChildAsync(string parentId, TModelCreate item, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            var id = await Logic.CreateChildAsync(parentId, item, token);
            FulcrumAssert.IsNotNullOrWhiteSpace(id, CodeLocation.AsString());
            return id;
        }

        /// <inheritdoc />
        public async Task<TModel> CreateChildAndReturnAsync(string parentId, TModelCreate item, CancellationToken token = default)
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            ServiceContract.RequireNotNull(item, nameof(item));
            var createdItem = await Logic.CreateChildAndReturnAsync(parentId, item, token);
            FulcrumAssert.IsNotNull(createdItem, CodeLocation.AsString());
            FulcrumAssert.IsValidated(createdItem, CodeLocation.AsString());
            return createdItem;
        }
    }
}