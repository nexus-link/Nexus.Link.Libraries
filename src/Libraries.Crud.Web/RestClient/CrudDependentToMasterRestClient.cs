using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.RestClient
{
    /// <inheritdoc cref="CrudDependentToMasterRestClient{TModelCreate,TModel,TId, TDependentId}" />
    public class CrudDependentToMasterRestClient<TModel, TId, TDependentId> :
        CrudDependentToMasterRestClient<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMaster<TModel, TId, TDependentId>
    {

        /// <summary></summary>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpSender">How to actually send HTTP requests.</param>
        public CrudDependentToMasterRestClient(string parentName, string childrenName, IHttpSender httpSender)
            : base(parentName, childrenName, httpSender)
        {
        }
    }

    /// <inheritdoc cref="Nexus.Link.Libraries.Crud.Web.RestClient" />
    public class CrudDependentToMasterRestClient<TManyModelCreate, TManyModel, TId, TDependentId> : 
        Libraries.Web.RestClientHelper.RestClient, 
        ICrudDependentToMaster<TManyModelCreate, TManyModel, TId, TDependentId> 
        where TManyModel : TManyModelCreate
    {
        private readonly DependentToMasterConvenience<TManyModelCreate, TManyModel, TId, TDependentId> _convenience;

        /// <summary>
        /// The name of the sub path that is the parent of the children. (Singular)
        /// </summary>
        protected string ParentName { get; }

        /// <summary>
        /// The name of the sub path that are the children. (Plural noun)
        /// </summary>
        public string ChildrenName { get; }

        /// <summary></summary>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpSender">How to actually send HTTP requests.</param>
        public CrudDependentToMasterRestClient(string parentName, string childrenName, IHttpSender httpSender)
            : base(httpSender)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
            _convenience = new DependentToMasterConvenience<TManyModelCreate, TManyModel, TId, TDependentId>(this);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(TId masterId, TDependentId dependentId, TManyModelCreate item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            return PostNoResponseContentAsync($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TDependentId dependentId, TManyModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            return PostAsync<TManyModel, TManyModelCreate>($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> ReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            return GetAsync<TManyModel>($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TManyModel>> ReadChildrenAsync(TId parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return GetAsync<IEnumerable<TManyModel>>($"{ParentName}/{parentId}/{ChildrenName}?limit={limit}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> ReadChildrenWithPagingAsync(TId parentId, int offset = 0, int? limit = null, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            var limitParameter = "";
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
                limitParameter = $"&limit={limit}";
            }
            return GetAsync<PageEnvelope<TManyModel>>($"{ParentName}/{parentId}/{ChildrenName}?offset={offset}{limitParameter}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> SearchChildrenAsync(TId parentId, SearchDetails<TManyModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            var limitParameter = "";
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
                limitParameter = $"&limit={limit}";
            }
            return PostAsync<PageEnvelope<TManyModel>, SearchDetails<TManyModel>>($"{ParentName}/{parentId}/{ChildrenName}/Searches?offset={offset}{limitParameter}", details, null,cancellationToken );
        }

        /// <inheritdoc />
        public Task<TManyModel> FindUniqueChildAsync(TId parentId, SearchDetails<TManyModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(TId masterId, TDependentId dependentId, TManyModel item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            if (item is IUniquelyIdentifiableDependent<TId, TDependentId> combinedId)
            {
                InternalContract.RequireAreEqual(masterId, combinedId.MasterId, $"{nameof(item)}.{nameof(combinedId.MasterId)}");
                InternalContract.RequireAreEqual(dependentId, combinedId.DependentId, $"{nameof(item)}.{nameof(combinedId.DependentId)}");
            }
            return PutNoResponseContentAsync($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> UpdateAndReturnAsync(TId masterId, TDependentId dependentId, TManyModel item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            return PostAsync<TManyModel, TManyModelCreate>($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId masterId, TDependentId dependentId, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            return DeleteAsync($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(TId parentId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            return DeleteAsync($"{ParentName}/{parentId}/{ChildrenName}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<DependentLock<TId, TDependentId>> ClaimDistributedLockAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            return PostAsync<DependentLock<TId, TDependentId>>($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}/Locks", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId masterId, TDependentId dependentId, TId lockId,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(dependentId, nameof(dependentId));
            InternalContract.RequireNotDefaultValue(lockId, nameof(lockId));
            return DeleteAsync($"{ParentName}/{masterId}/{ChildrenName}/{dependentId}/Locks", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TManyModel> ClaimTransactionLockAndReadAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TId> GetDependentUniqueIdAsync(TId masterId, TDependentId dependentId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
