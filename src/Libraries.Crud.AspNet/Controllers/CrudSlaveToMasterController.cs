using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;
using Nexus.Link.Libraries.Web.AspNet.Annotations;

namespace Nexus.Link.Libraries.Crud.AspNet.Controllers
{
    /// <inheritdoc cref="CrudSlaveToMasterController{TModelCreate,TModel}" />
    [Obsolete("Use Nexus.Link.Libraries.Crud.AspNet.ControllerHelpers. Obsolete since 2020-09-23.")]
    public abstract class CrudSlaveToMasterController<TModel> :
        CrudSlaveToMasterController<TModel, TModel>,
        ICrudSlaveToMaster<TModel, string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected CrudSlaveToMasterController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <inheritdoc cref="CrudControllerBase" />
    [Obsolete("Use Nexus.Link.Libraries.Crud.AspNet.ControllerHelpers. Obsolete since 2020-09-23.")]
    public abstract class CrudSlaveToMasterController<TModelCreate, TModel> :
        CrudControllerBase,
        ICrudSlaveToMaster<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        /// <summary>
        /// The logic to be used
        /// </summary>
        protected readonly ICrudSlaveToMaster<TModelCreate, TModel, string> Logic;

        /// <summary>
        /// Constructor
        /// </summary>
        protected CrudSlaveToMasterController(ICrudable<TModel, string> logic)
        {
            Logic = new SlaveToMasterPassThrough<TModelCreate, TModel, string>(logic);
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<string> CreateAsync(string masterId, TModelCreate item, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotDefaultValue(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            return await Logic.CreateAsync(masterId, item, token);
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<TModel> CreateAndReturnAsync(string masterId, TModelCreate item, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotDefaultValue(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            var createdItem = await Logic.CreateAndReturnAsync(masterId, item, token);
            FulcrumAssert.IsNotNull(createdItem);
            FulcrumAssert.IsValidated(createdItem);
            return createdItem;
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task CreateWithSpecifiedIdAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            ServiceContract.RequireNotDefaultValue(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            await Logic.CreateWithSpecifiedIdAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            ServiceContract.RequireNotDefaultValue(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            var createdItem = await Logic.CreateWithSpecifiedIdAndReturnAsync(masterId, slaveId, item, token);
            FulcrumAssert.IsNotNull(createdItem);
            FulcrumAssert.IsValidated(createdItem);
            return createdItem;
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<TModel> ReadAsync(string masterId, string slaveId, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            var item = await Logic.ReadAsync(masterId, slaveId, token);
            FulcrumAssert.IsNotNull(item);
            FulcrumAssert.IsValidated(item);
            return item;
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string masterId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                ServiceContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }

            var page = await Logic.ReadChildrenWithPagingAsync(masterId, offset, limit, token);
            FulcrumAssert.IsNotNull(page?.Data);
            FulcrumAssert.IsValidated(page?.Data);
            return page;
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
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
        public virtual Task<TModel> ReadAsync(SlaveToMasterId<string> id, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNull(id, nameof(id));
            ServiceContract.RequireValidated(id, nameof(id));
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task UpdateAsync(string masterId, string slaveId, TModel item, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            ServiceContract.RequireNotDefaultValue(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            await Logic.UpdateAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task<TModel> UpdateAndReturnAsync(string masterId, string slaveId, TModel item,
            CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            ServiceContract.RequireNotDefaultValue(item, nameof(item));
            ServiceContract.RequireValidated(item, nameof(item));
            var createdItem = await Logic.UpdateAndReturnAsync(masterId, slaveId, item, token);
            FulcrumAssert.IsNotNull(createdItem);
            FulcrumAssert.IsValidated(createdItem);
            return createdItem;
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual Task DeleteAsync(string masterId, string slaveId, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            return Logic.DeleteAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        [SwaggerBadRequestResponse]
        [SwaggerInternalServerErrorResponse]
        public virtual async Task DeleteChildrenAsync(string masterId, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            await Logic.DeleteChildrenAsync(masterId, token);
        }

        /// <inheritdoc />
        public virtual Task<SlaveLock<string>> ClaimLockAsync(string masterId, string slaveId, CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            return Logic.ClaimLockAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public virtual Task ReleaseLockAsync(string masterId, string slaveId, string lockId,
            CancellationToken token = new CancellationToken())
        {
            ServiceContract.RequireNotNullOrWhiteSpace(masterId, nameof(masterId));
            ServiceContract.RequireNotNullOrWhiteSpace(slaveId, nameof(slaveId));
            ServiceContract.RequireNotNullOrWhiteSpace(lockId, nameof(lockId));
            return Logic.ReleaseLockAsync(masterId, slaveId, lockId, token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<string>> ClaimDistributedLockAsync(string masterId, string slaveId, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(string masterId, string slaveId, string lockId,
            CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(string masterId, string slaveId, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(string parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> SearchFirstChildAsync(string parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(string parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}