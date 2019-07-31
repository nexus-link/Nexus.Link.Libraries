using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.Web.RestClient
{
    /// <inheritdoc cref="CrudManyToOneRestClient{TManyModelCreate, TManyModel,TId}" />
    public class CrudManyToOneRestClient<TManyModel, TId> : 
        CrudManyToOneRestClient<TManyModel, TManyModel, TId>,
        ICrudManyToOne<TManyModel, TId>
    {

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        public CrudManyToOneRestClient(string baseUri, string parentName = "Parent", string childrenName = "Children")
            : base(baseUri, parentName, childrenName)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="credentials">The credentials used when making the HTTP calls</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        public CrudManyToOneRestClient(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials, string parentName = "Parent", string childrenName = "Children")
            : base(baseUri, httpClient, credentials, parentName, childrenName)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="parentName">The name of the sub path that is the parent of the children. (Singular noun)</param>
        /// <param name="childrenName">The name of the sub path that are the children. (Plural noun)</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        public CrudManyToOneRestClient(string baseUri, HttpClient httpClient, string parentName, string childrenName)
            : base(baseUri, httpClient, parentName, childrenName)
        {
        }
    }

    /// <inheritdoc cref="CrudRestClient{TManyModelCreate, TManyModel,TId}" />
    public class CrudManyToOneRestClient<TManyModelCreate, TManyModel, TId> : 
        CrudRestClient<TManyModelCreate, TManyModel, TId>, 
        ICrudManyToOne<TManyModelCreate, TManyModel, TId> where TManyModel : TManyModelCreate
    {
        /// <summary>
        /// The name of the sub path that is the parent of the children. (Singular noun)
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
        public CrudManyToOneRestClient(string baseUri, string parentName, string childrenName)
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
        public CrudManyToOneRestClient(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials, string parentName, string childrenName)
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
        public CrudManyToOneRestClient(string baseUri, HttpClient httpClient, string parentName, string childrenName)
            : base(baseUri, httpClient)
        {
            ParentName = parentName;
            ChildrenName = childrenName;
        }

        /// <inheritdoc />
        public virtual async Task DeleteChildrenAsync(TId parentId, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            await DeleteAsync($"{parentId}/{ChildrenName}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TManyModel>> ReadChildrenAsync(TId parentId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return await GetAsync<IEnumerable<TManyModel>>($"{parentId}/{ChildrenName}?limit={limit}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TManyModel>> ReadChildrenWithPagingAsync(TId parentId, int offset = 0, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            var limitParameter = "";
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
                limitParameter = $"&limit={limit}";
            }
            return await GetAsync<PageEnvelope<TManyModel>>($"{parentId}/{ChildrenName}?offset={offset}{limitParameter}", cancellationToken: token);
        }
    }
}
