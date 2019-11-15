﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.Web.RestClient
{
    /// <inheritdoc cref="CrudSlaveToMasterRestClient{TModelCreate,TModel,TId}" />
    public class CrudSlaveToMasterRestClient<TModel, TId> :
        CrudSlaveToMasterRestClient<TModel, TModel, TId>,
        ICrudSlaveToMaster<TModel, TId>
    {
        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudSlaveToMasterRestClient(string baseUri, string parentName, string childrenName)
            : base(baseUri, parentName, childrenName)
        {
        }
        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudSlaveToMasterRestClient(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials, string parentName, string childrenName)
            : base(baseUri, httpClient, credentials, parentName, childrenName)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudSlaveToMasterRestClient(string baseUri, HttpClient httpClient, string parentName, string childrenName)
            : base(baseUri, httpClient, parentName, childrenName)
        {
        }
    }

    /// <inheritdoc cref="RestClient" />
    public class CrudSlaveToMasterRestClient<TManyModelCreate, TManyModel, TId> : 
        Libraries.Web.RestClientHelper.RestClient, 
        ICrudSlaveToMaster<TManyModelCreate, TManyModel, TId> 
        where TManyModel : TManyModelCreate
    {
        /// <summary>
        /// The name of the sub path that is the parent of the children. (Singular)
        /// </summary>
        protected string ParentName { get; }

        /// <summary>
        /// The name of the sub path that are the children. (Plural noun)
        /// </summary>
        public string ChildrenName { get; }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudSlaveToMasterRestClient(string baseUri, string parentName, string childrenName)
            : base(baseUri)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudSlaveToMasterRestClient(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials, string parentName, string childrenName)
            : base(baseUri, httpClient, credentials)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudSlaveToMasterRestClient(string baseUri, HttpClient httpClient, string parentName, string childrenName)
            : base(baseUri, httpClient)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
        }

        /// <inheritdoc />
        public Task<TId> CreateAsync(TId masterId, TManyModelCreate item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            return PostAsync<TId, TManyModelCreate>($"{masterId}/{ChildrenName}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateAndReturnAsync(TId masterId, TManyModelCreate item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            return PostAsync<TManyModel, TManyModelCreate>($"{masterId}/{ChildrenName}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(TId masterId, TId slaveId, TManyModelCreate item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            return PostNoResponseContentAsync($"{masterId}/{ChildrenName}/{slaveId}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TId slaveId, TManyModelCreate item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            return PostAsync<TManyModel, TManyModelCreate>($"{masterId}/{ChildrenName}/{slaveId}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> ReadAsync(TId masterId, TId slaveId, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            return GetAsync<TManyModel>($"{masterId}/{ChildrenName}/{slaveId}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> ReadAsync(SlaveToMasterId<TId> id, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotNull(id, nameof(id));
            InternalContract.RequireValidated(id, nameof(id));
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TManyModel>> ReadChildrenAsync(TId parentId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return GetAsync<IEnumerable<TManyModel>>($"{parentId}/{ChildrenName}?limit={limit}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> ReadChildrenWithPagingAsync(TId parentId, int offset = 0, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            var limitParameter = "";
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
                limitParameter = $"&limit={limit}";
            }
            return GetAsync<PageEnvelope<TManyModel>>($"{parentId}/{ChildrenName}?offset={offset}{limitParameter}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task UpdateAsync(TId masterId, TId slaveId, TManyModel item, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            return PutNoResponseContentAsync($"{masterId}/{ChildrenName}/{slaveId}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> UpdateAndReturnAsync(TId masterId, TId slaveId, TManyModel item,
            CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            return PostAsync<TManyModel, TManyModelCreate>($"{masterId}/{ChildrenName}/{slaveId}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId masterId, TId slaveId, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            return DeleteAsync($"{masterId}/{ChildrenName}/{slaveId}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(TId parentId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            return DeleteAsync($"{parentId}/{ChildrenName}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<TId>> ClaimLockAsync(TId masterId, TId slaveId, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            return PostAsync<SlaveLock<TId>>($"{masterId}/{ChildrenName}/{slaveId}/Locks", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId masterId, TId slaveId, TId lockId, CancellationToken token = new CancellationToken())
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotDefaultValue(lockId, nameof(lockId));
            return DeleteAsync($"{masterId}/{ChildrenName}/{slaveId}/Locks", cancellationToken: token);
        }
    }
}
