using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.RestClient
{
    /// <inheritdoc cref="CrudManyToOneRestClient{TManyModelCreate, TManyModel,TId}" />
    public class CrudManyToOneRestClient2<TManyModel, TId> : 
        CrudManyToOneRestClient2<TManyModel, TManyModel, TId>,
        ICrudManyToOne<TManyModel, TId>
    {
        /// <summary></summary>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpSender">How to actually send HTTP requests.</param>
        public CrudManyToOneRestClient2(IHttpSender httpSender, string parentName = "Parent", string childrenName = "Children")
            : base(httpSender, parentName, childrenName)
        {
        }

        #region Obsolete constructors

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete warning since 2019-11-18, error since 2021-06-09.", true)]
        public CrudManyToOneRestClient2(string baseUri, string parentName = "Parent", string childrenName = "Children")
            : base(baseUri, parentName, childrenName)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="credentials">The credentials used when making the HTTP calls</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete warning since 2019-11-18, error since 2021-06-09.", true)]
        public CrudManyToOneRestClient2(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials, string parentName = "Parent", string childrenName = "Children")
            : base(baseUri, httpClient, credentials, parentName, childrenName)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete warning since 2019-11-18, error since 2021-06-09.", true)]
        public CrudManyToOneRestClient2(string baseUri, HttpClient httpClient, string parentName, string childrenName)
            : base(baseUri, httpClient, parentName, childrenName)
        {
        }
        #endregion
    }

    /// <inheritdoc cref="ICrudManyToOne{TManyModelCreate, TManyModel,TId}" />
    public class CrudManyToOneRestClient2<TManyModelCreate, TManyModel, TId> :
        Libraries.Web.RestClientHelper.RestClient,
        ICrudManyToOne<TManyModelCreate, TManyModel, TId> where TManyModel : TManyModelCreate
    {
        private readonly ManyToOneConvenience<TManyModelCreate, TManyModel, TId> _convenience;
        /// <summary>
        /// The name of the sub path that is the parent of the children. (Singular noun)
        /// </summary>
        protected string ParentName { get; }

        /// <summary>
        /// The name of the sub path that are the children. (Plural noun)
        /// </summary>
        public string ChildrenName { get; }

        protected CrudRestClient<TManyModelCreate, TManyModel, TId> CrudRestClient { get; }

        /// <summary></summary>
        /// <param name="httpSender">How to actually send HTTP requests.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Plural noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudManyToOneRestClient2(IHttpSender httpSender, string parentName, string childrenName)
        :base(httpSender)
        {
            CrudRestClient = new
                CrudRestClient<TManyModelCreate, TManyModel, TId>(httpSender.CreateHttpSender(childrenName));
            ParentName = parentName;
            ChildrenName = childrenName;
            _convenience = new ManyToOneConvenience<TManyModelCreate, TManyModel, TId>(this);
        }

        #region Obsolete constructors

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete warning since 2019-11-18, error since 2021-06-09.", true)]
        public CrudManyToOneRestClient2(string baseUri, string parentName, string childrenName)
            : base(baseUri)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="credentials">The credentials used when making the HTTP calls</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete warning since 2019-11-18, error since 2021-06-09.", true)]
        public CrudManyToOneRestClient2(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials,
            string parentName, string childrenName)
            : base(baseUri, httpClient, credentials)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete warning since 2019-11-18, error since 2021-06-09.", true)]
        public CrudManyToOneRestClient2(string baseUri, HttpClient httpClient, string parentName, string childrenName)
            : base(baseUri, httpClient)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
        }
        #endregion

        /// <inheritdoc />
        public Task<TId> CreateChildAsync(TId parentId, TManyModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            return PostAsync<TId, TManyModelCreate>($"{ParentName}/{parentId}/{ChildrenName}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateChildAndReturnAsync(TId parentId, TManyModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            return PostAsync<TManyModel, TManyModelCreate>($"{ParentName}/{parentId}/{ChildrenName}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task CreateChildWithSpecifiedIdAsync(TId parentId, TId childId, TManyModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            return PostAsync<TId, TManyModelCreate>($"{ParentName}/{parentId}/{ChildrenName}/{childId}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateChildWithSpecifiedIdAndReturnAsync(TId parentId, TId childId, TManyModelCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            return PostAsync<TManyModel, TManyModelCreate>($"{ParentName}/{parentId}/{ChildrenName}/{childId}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task DeleteChildrenAsync(TId parentId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            await DeleteAsync($"{ParentName}/{parentId}/{ChildrenName}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TManyModel>> ReadChildrenAsync(TId parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return await GetAsync<IEnumerable<TManyModel>>($"{ParentName}/{parentId}/{ChildrenName}?limit={limit}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TManyModel>> ReadChildrenWithPagingAsync(TId parentId, int offset = 0, int? limit = null, CancellationToken token = default)
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            var limitParameter = "";
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
                limitParameter = $"&limit={limit}";
            }
            return await GetAsync<PageEnvelope<TManyModel>>($"{ParentName}/{parentId}/{ChildrenName}?offset={offset}{limitParameter}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> SearchChildrenAsync(TId parentId, SearchDetails<TManyModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
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
        public Task<TManyModel> CreateAndReturnAsync(TManyModelCreate item, CancellationToken token = default)
        {
            return CrudRestClient.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(TId id, TManyModelCreate item, CancellationToken token = default)
        {
            return CrudRestClient.CreateWithSpecifiedIdAsync(id, item, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TManyModelCreate item, CancellationToken token = default)
        {
            return CrudRestClient.CreateWithSpecifiedIdAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TManyModel>> ReadAllAsync(int limit = Int32.MaxValue, CancellationToken token = default)
        {
            return CrudRestClient.ReadAllAsync(limit, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> SearchAsync(SearchDetails<TManyModel> details, int offset, int? limit = null, CancellationToken cancellationToken = default)
        {
            return CrudRestClient.SearchAsync(details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TManyModel> FindUniqueAsync(SearchDetails<TManyModel> details, CancellationToken cancellationToken = default)
        {
            return CrudRestClient.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TManyModel> UpdateAndReturnAsync(TId id, TManyModel item, CancellationToken token = default)
        {
            return CrudRestClient.UpdateAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = default)
        {
            return CrudRestClient.ClaimLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TId id, TId lockId, CancellationToken token = default)
        {
            return CrudRestClient.ReleaseLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public Task<Lock<TId>> ClaimDistributedLockAsync(TId id, CancellationToken token = default)
        {
            return CrudRestClient.ClaimDistributedLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TId objectId, TId lockId, CancellationToken token = default)
        {
            return CrudRestClient.ClaimDistributedLockAsync(objectId, token);
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken token = default)
        {
            return CrudRestClient.ClaimTransactionLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken token = default)
        {           
            return CrudRestClient.ClaimTransactionLockAndReadAsync(id, token);
        }

        /// <inheritdoc />
        public Task<TId> CreateAsync(TManyModelCreate item, CancellationToken token = default)
        {
            return CrudRestClient.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public Task<TManyModel> ReadAsync(TId id, CancellationToken token = default)
        {
            return CrudRestClient.ReadAsync(id, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TManyModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default)
        {
            return CrudRestClient.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public Task UpdateAsync(TId id, TManyModel item, CancellationToken token = default)
        {
            return CrudRestClient.UpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(TId id, CancellationToken token = default)
        {
            return CrudRestClient.DeleteAsync(id, token);
        }

        /// <inheritdoc />
        public Task DeleteAllAsync(CancellationToken token = default)
        {
            return CrudRestClient.DeleteAllAsync(token);
        }
    }
}
