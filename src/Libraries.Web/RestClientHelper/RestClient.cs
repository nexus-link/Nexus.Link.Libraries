using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.Libraries.Web.Pipe.Outbound;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <summary>
    /// Convenience client for making REST calls
    /// </summary>
    public class RestClient : IRestClient
    {
        public IHttpSender HttpSender { get; }

        private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");

        /// <inheritdoc />
        public RestClient(IHttpSender httpSender)
        {
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            HttpSender = httpSender;
        }

        #region Obsolete constructors
        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        // ReSharper disable once UnusedParameter.Local
        [Obsolete("Use the RestClient(IHttpSender) constructor. Obsolete since 2019-11-15.")]
        public RestClient(string baseUri) : this(new HttpSender(baseUri))
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        [Obsolete("Use the RestClient(IHttpSender) constructor. Obsolete since 2019-11-15.")]
        public RestClient(string baseUri, ServiceClientCredentials credentials) : this(new HttpSender(baseUri, credentials))
        {
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        [Obsolete("Use the RestClient(IHttpSender) constructor. Obsolete since 2019-11-15.")]
        public RestClient(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials) : this(new HttpSender(baseUri, httpClient, credentials))
        {
        }


        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        [Obsolete("Use the RestClient(IHttpSender) constructor. Obsolete since 2019-11-15.")]
        public RestClient(string baseUri, HttpClient httpClient) : this(new HttpSender(baseUri, httpClient))
        {
        }
        #endregion

        /// <inheritdoc />
        public Uri BaseUri => HttpSender.BaseUri;

        #region POST
        /// <inheritdoc />
        public async Task<TResponse> PostAsync<TResponse, TBody>(string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync<TResponse, TBody>(HttpMethod.Post, relativeUrl, body, customHeaders, cancellationToken);
            return response.Body;
        }

        /// <inheritdoc />
        public async Task<TResponse> PostAsync<TResponse>(string relativeUrl, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            return await PostAsync<TResponse, string>(relativeUrl, null, customHeaders, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TBodyAndResponse> PostAndReturnCreatedObjectAsync<TBodyAndResponse>(string relativeUrl, TBodyAndResponse body,
            Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            return await PostAsync<TBodyAndResponse, TBodyAndResponse>(relativeUrl, body, customHeaders, cancellationToken);
        }

        /// <inheritdoc />
        public async Task PostNoResponseContentAsync<TBody>(string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync(HttpMethod.Post, relativeUrl, body, customHeaders, cancellationToken);
            await VerifySuccessAsync(response);
        }

        /// <inheritdoc />
        public async Task PostNoResponseContentAsync(string relativeUrl, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync(HttpMethod.Post, relativeUrl, customHeaders, cancellationToken);
            await VerifySuccessAsync(response);
        }
        #endregion

        #region GET

        /// <inheritdoc />
        public async Task<TResponse> GetAsync<TResponse>(string relativeUrl, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync<TResponse, object>(HttpMethod.Get, relativeUrl, null, customHeaders, cancellationToken);
            return response.Body;
        }
        #endregion

        #region PUT

        /// <inheritdoc />
        public Task<TResponse> PutAsync<TResponse, TBody>(string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return PutOrPatchAsync<TResponse, TBody>(HttpMethod.Put, relativeUrl, body, customHeaders, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TBodyAndResponse> PutAndReturnUpdatedObjectAsync<TBodyAndResponse>(string relativeUrl, TBodyAndResponse body,
            Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return PutOrPatchAndReturnUpdatedObjectAsync(HttpMethod.Put, relativeUrl, body, customHeaders, cancellationToken);
        }

        /// <inheritdoc />
        public Task PutNoResponseContentAsync<TBody>(string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return PutOrPatchNoResponseContentAsync(HttpMethod.Put, relativeUrl, body, customHeaders, cancellationToken);
        }

        #endregion

        #region PATCH

        /// <inheritdoc />
        public Task<TResponse> PatchAsync<TResponse, TBody>(string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return PutOrPatchAsync<TResponse, TBody>(PatchMethod, relativeUrl, body, customHeaders, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TBodyAndResponse> PatchAndReturnUpdatedObjectAsync<TBodyAndResponse>(string relativeUrl, TBodyAndResponse body,
            Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = new CancellationToken())
        {
            return PutOrPatchAndReturnUpdatedObjectAsync(PatchMethod, relativeUrl, body, customHeaders, cancellationToken);
        }

        /// <inheritdoc />
        public Task PatchNoResponseContentAsync<TBody>(string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return PutOrPatchNoResponseContentAsync(PatchMethod, relativeUrl, body, customHeaders, cancellationToken);
        }

        #endregion

        #region DELETE

        /// <inheritdoc />
        public async Task DeleteAsync(string relativeUrl, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync(HttpMethod.Delete, relativeUrl, customHeaders, cancellationToken);
            await VerifySuccessAsync(response);
        }
        #endregion

        #region Send
        /// <inheritdoc />
        public Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return HttpSender.SendRequestAsync<TResponse, TBody>(method, relativeUrl, body, customHeaders,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return HttpSender.SendRequestAsync(method, relativeUrl, body, customHeaders,
                cancellationToken);
        }

        /// <inheritdoc />
        public IHttpSender CreateHttpSender(string relativeUrl)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            try
            {
                var newUri = new Uri(BaseUri, relativeUrl);
                return new RestClient(HttpSender.CreateHttpSender(relativeUrl));
            }
            catch (UriFormatException e)
            {
                InternalContract.Fail($"The format of {nameof(relativeUrl)} ({relativeUrl}) is not correct: {e.Message}");
                return null;
            }
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return HttpSender.SendRequestAsync(method, relativeUrl, customHeaders, cancellationToken);
        }
        #endregion

        #region Helpers

        private async Task<TResponse> PutOrPatchAsync<TResponse, TBody>(HttpMethod method, string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync<TResponse, TBody>(method, relativeUrl, body, customHeaders, cancellationToken);
            return response.Body;
        }

        private async Task<TBodyAndResponse> PutOrPatchAndReturnUpdatedObjectAsync<TBodyAndResponse>(HttpMethod method, string relativeUrl, TBodyAndResponse body,
            Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            return await PutOrPatchAsync<TBodyAndResponse, TBodyAndResponse>(method, relativeUrl, body, customHeaders, cancellationToken);
        }

        private async Task PutOrPatchNoResponseContentAsync<TBody>(HttpMethod method, string relativeUrl, TBody body, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken);
            await VerifySuccessAsync(response);
        }

        private async Task VerifySuccessAsync(HttpResponseMessage response)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            InternalContract.RequireNotNull(response.RequestMessage, $"{nameof(response)}.{nameof(response.RequestMessage)}");
            if (!response.IsSuccessStatusCode)
            {
                var requestContent = await TryGetContentAsString(response.RequestMessage?.Content, true);
                var responseContent = await TryGetContentAsString(response.Content, true);
                var message = $"{response.StatusCode} {responseContent}";
                var exception = new HttpOperationException(message)
                {
                    Response = new HttpResponseMessageWrapper(response, responseContent),
                    Request = new HttpRequestMessageWrapper(response.RequestMessage, requestContent)
                };
                throw exception;
            }
        }

        private async Task<string> TryGetContentAsString(HttpContent content, bool silentlyIgnoreExceptions)
        {
            if (content == null) return null;
            try
            {
                return await content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!silentlyIgnoreExceptions) throw new FulcrumAssertionFailedException("Expected to be able to read an HttpContent.", e);
            }
            return null;
        }
        #endregion
    }
}
