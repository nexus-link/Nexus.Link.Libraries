using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Helpers;
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
        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="httpSender"></param>
        public CrudRestClient(IHttpSender httpSender)
            : base(httpSender)
        {
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
    }

    /// <inheritdoc cref="RestClient" />
    public class CrudRestClient<TModelCreate, TModel, TId> : 
        Libraries.Web.RestClientHelper.RestClient,
        ICrud<TModelCreate, TModel, TId> where TModel : TModelCreate
    {
        private readonly CrudConvenience<TModelCreate, TModel, TId> _convenience;

        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="httpSender"></param>
        public CrudRestClient(IHttpSender httpSender)
            : base(httpSender)
        {
            _convenience = new CrudConvenience<TModelCreate, TModel,TId>(this);
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
        public virtual async Task<TId> CreateAsync(TModelCreate item, CancellationToken token = default)
        {
            // TODO: PostAndDecorateResultAsync
            var invoiceId = await PostAsync<TId, TModelCreate>("", item, cancellationToken: token);
            return MaybeDecorate(invoiceId);
        }

        private TId MaybeDecorate(TId invoiceId)
        {
            if (typeof(TId) != typeof(string)) return invoiceId;
            if (!(HttpSender is ITranslationClientName translationTargetClientName)) return invoiceId;
            if (!typeof(IUniquelyIdentifiable<string>).IsAssignableFrom(typeof(TModel))) return invoiceId;

            var idPropertyInfo = typeof(TModel).GetProperty(nameof(IUniquelyIdentifiable<string>.Id));
            var translationConcept = Translator.GetConceptAttribute(idPropertyInfo);
            if (translationConcept == null) return invoiceId;

            return (TId)(object) Translator.Decorate(
                translationConcept.ConceptName,
                translationTargetClientName.TranslationClientName, 
                (string)(object)invoiceId);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default)
        {
            return await PostAsync<TModel, TModelCreate>("ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task CreateWithSpecifiedIdAsync(TId id, TModelCreate item, CancellationToken token = default)
        {
            await PostNoResponseContentAsync($"{id}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(TId id, TModelCreate item, CancellationToken token = default)
        {
            return await PostAsync<TModel, TModelCreate>($"{id}/ReturnCreated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> ReadAsync(TId id, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(id, nameof(id));
            return await GetAsync<TModel>($"{id}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset = 0, int? limit = null, CancellationToken token = default)
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
        public virtual async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken token = default)
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return await GetAsync<IEnumerable<TModel>>($"?limit={limit}", cancellationToken: token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            ServiceContract.RequireNotNull(details, nameof(details));
            ServiceContract.RequireValidated(details, nameof(details));
            ServiceContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            if (limit != null)
            {
                ServiceContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            }
            var limitParameter = "";
            if (limit != null)
            {
                limitParameter = $"&limit={limit}";
            }
            return await PostAsync<PageEnvelope<TModel>, SearchDetails<TModel>>($"/Searches?offset={offset}{limitParameter}", details, cancellationToken: cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueAsync(details, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TId id, TModel item, CancellationToken token = default)
        {
            await PutNoResponseContentAsync($"{id}", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> UpdateAndReturnAsync(TId id, TModel item, CancellationToken token = default)
        {
            return await PutAndReturnUpdatedObjectAsync($"{id}/ReturnUpdated", item, cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId id, CancellationToken token = default)
        {
            await DeleteAsync($"{id}", cancellationToken: token);
        }

        /// <inheritdoc />
        public virtual async Task DeleteAllAsync(CancellationToken token = default)
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

        /// <inheritdoc />
        public async Task<Lock<TId>> ClaimDistributedLockAsync(TId id, CancellationToken token = default)
        {
            return await PostAsync<Lock<TId>>($"{id}/Locks", cancellationToken: token);
        }

        /// <inheritdoc />
        public async Task ReleaseDistributedLockAsync(TId id, TId lockId, CancellationToken token = default)
        {
            await PostNoResponseContentAsync($"{id}/Locks/{lockId}", cancellationToken: token);
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TId id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(TId id, CancellationToken token = default)
        {
            return _convenience.ClaimTransactionLockAndReadAsync(id, token);
        }
    }
}
