using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;
using Nexus.Link.Libraries.Web.AspNet.Annotations;

namespace Nexus.Link.Libraries.Crud.AspNet.Controllers
{
    /// <inheritdoc cref="CrudManyToOneController{TModelCreate,TModel}" />
    [Obsolete("Use Nexus.Link.Libraries.Crud.AspNet.ControllerHelpers. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public abstract class CrudManyToOneController<TModel> :
        CrudManyToOneController<TModel, TModel>,
        ICrudManyToOne<TModel, string>
    {
        /// <inheritdoc />
        protected CrudManyToOneController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <inheritdoc cref="CrudController{TModelCreate,TModel}" />
    [Obsolete("Use Nexus.Link.Libraries.Crud.AspNet.ControllerHelpers. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public abstract class CrudManyToOneController<TModelCreate, TModel> :
        CrudController<TModelCreate, TModel>,
        ICrudManyToOne<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        /// <summary>
        /// The logic to be used
        /// </summary>
        protected new readonly ICrudManyToOne<TModelCreate, TModel, string> Logic;

        /// <summary>
        /// Constructor
        /// </summary>
        protected CrudManyToOneController(ICrudable<TModel, string> logic)
        : base(logic)
        {
            Logic = new ManyToOnePassThrough<TModelCreate, TModel, string>(logic);
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
            CancellationToken token = new CancellationToken())
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
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            ServiceContract.RequireGreaterThan(0, limit, nameof(limit));
            var items = await Logic.ReadChildrenAsync(parentId, limit, token);
            FulcrumAssert.IsNotNull(items);
            FulcrumAssert.IsValidated(items);
            return items;
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task DeleteChildrenAsync(string parentId, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(parentId, nameof(parentId));
            await Logic.DeleteChildrenAsync(parentId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(string parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(string parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<string> CreateChildAsync(string parentId, TModelCreate item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> CreateChildAndReturnAsync(string parentId, TModelCreate item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task CreateChildWithSpecifiedIdAsync(string parentId, string childId, TModelCreate item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> CreateChildWithSpecifiedIdAndReturnAsync(string parentId, string childId, TModelCreate item,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}