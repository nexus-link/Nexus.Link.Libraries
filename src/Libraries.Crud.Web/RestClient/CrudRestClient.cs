using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.RestClient
{
    /// <inheritdoc cref="CrudRestClient{TModelCreate,TModel,TId}" />
    public class CrudRestClient<TModel, TId> : 
        CrudRestClient<TModel, TModel, TId>,
        ICrud<TModel, TId>
    {
        #region Obsolete constructors
        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete since 2019-11-18")]
        public CrudRestClient(string baseUri)
            : base(baseUri)
        {
        }
        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete since 2019-11-18")]
        public CrudRestClient(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials)
            : base(baseUri, httpClient, credentials)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete since 2019-11-18")]
        public CrudRestClient(string baseUri, HttpClient httpClient)
            : base(baseUri, httpClient)
        {
        }
        #endregion

        /// <inheritdoc />
        public CrudRestClient(IHttpSender httpSender)
            : base(httpSender)
        {
        }
    }

    /// <inheritdoc cref="RestClient" />
    public class CrudRestClient<TModelCreate, TModel, TId> : 
        Libraries.Web.RestClientHelper.RestClient,
        ICrud<TModelCreate, TModel, TId> where TModel : TModelCreate
    {
        /// <summary>
        /// The name of the system that is called. Used for decorating the string result from CreateAsync.
        /// </summary>
        protected readonly string ProducerName;
        /// <summary>
        /// The concept name for the string result from CreateAsync.
        /// </summary>
        protected readonly string IdConceptName;

        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="httpSender"></param>
        /// <param name="idConceptName">The concept name for the string result from CreateAsync.</param>
        /// <param name="producerName">The name of the system that is called. Used for decorating the string result from CreateAsync.</param>
        /// <remarks>
        /// If you want intend to decorate results from the CreateAsync method you must set <paramref name="idConceptName"/> and <paramref name="producerName"/>.
        /// </remarks>
        public CrudRestClient(IHttpSender httpSender, string idConceptName = null, string producerName = null)
            : base(httpSender)
        {
            if (!string.IsNullOrWhiteSpace(producerName))
            {
                InternalContract.Require(!string.IsNullOrWhiteSpace(idConceptName),
                    $"When the parameter {nameof(producerName)} is not null, then the parameter {nameof(idConceptName)} must be non-null too.");
            }
            if (!string.IsNullOrWhiteSpace(idConceptName))
            {
                InternalContract.Require(!string.IsNullOrWhiteSpace(idConceptName),
                    $"When the parameter {nameof(idConceptName)} is not null, then the parameter {nameof(producerName)} must be non-null too.");
            }
            ProducerName = producerName;
            IdConceptName = idConceptName;
        }

        #region Obsolete constructors
        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete since 2019-11-18")]
        public CrudRestClient(string baseUri)
            : base(baseUri)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete since 2019-11-18")]
        public CrudRestClient(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials)
            : base(baseUri, httpClient, credentials)
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        [Obsolete("Use constructor with IHttpSender. Obsolete since 2019-11-18")]
        public CrudRestClient(string baseUri, HttpClient httpClient)
            : base(baseUri, httpClient)
        {
        }
        #endregion

        /// <inheritdoc />
        public virtual async Task<TId> CreateAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            // TODO: PostAndDecorateResultAsync
            var invoiceId = await PostAsync<TId, TModelCreate>("", item, cancellationToken: token);
            if (IdConceptName == null || ProducerName == null || typeof(TId) != typeof(string)) return invoiceId;
            return (TId)(object) Translator.Decorate(IdConceptName, ProducerName, (string)(object)invoiceId);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            return await PostAsync<TModel, TModelCreate>("ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            await PostNoResponseContentAsync($"{id}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            return await PostAsync<TModel, TModelCreate>($"{id}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> ReadAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return await GetAsync<TModel>($"{id}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset = 0, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            var limitParameter = "";
            if (limit != null)
            {
                InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
                limitParameter = $"&limit={limit}";
            }
            return await GetAsync<PageEnvelope<TModel>>($"?offset={offset}{limitParameter}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return await GetAsync<IEnumerable<TModel>>($"?limit={limit}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            await PutNoResponseContentAsync($"{id}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = default(CancellationToken))
        {
            return await PutAndReturnUpdatedObjectAsync($"{id}/ReturnUpdated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken token = default(CancellationToken))
        {
            await DeleteAsync($"{id}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            await DeleteAsync("", cancellationToken: token);
        }

        /// <inheritdoc />
        public async Task<Lock<TId>> ClaimLockAsync(TId id, CancellationToken token = new CancellationToken())
        {
            return await PostAsync<Lock<TId>>($"{id}/Locks", cancellationToken: token);
        }

        /// <inheritdoc />
        public async Task ReleaseLockAsync(TId id, TId lockId, CancellationToken token = new CancellationToken())
        {
            await PostNoResponseContentAsync($"{id}/Locks/{lockId}", cancellationToken: token);
        }
    }
}
