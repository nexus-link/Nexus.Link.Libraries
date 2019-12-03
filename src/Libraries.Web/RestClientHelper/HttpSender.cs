﻿using System;
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
using Nexus.Link.Libraries.Web.Logging;
using Nexus.Link.Libraries.Web.Pipe.Outbound;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <summary>
    /// Convenience client for making REST calls
    /// </summary>
    public class HttpSender : IHttpSender
    {

        private static readonly object LockClass = new object();

        /// <summary>
        /// The HttpClient that is used for all HTTP calls.
        /// </summary>
        /// <remarks>Is set to <see cref="HttpClient"/> by default. Typically only set to other values for unit test purposes.</remarks>
        public static IHttpClient HttpClient { get; set; }

        /// <inheritdoc />
        public Uri BaseUri { get; set; }

        /// <summary>
        /// Credentials that are used when sending requests to the service.
        /// </summary>
        protected ServiceClientCredentials Credentials { get; }

        /// <summary>
        /// Json settings when serializing to strings
        /// </summary>
        public JsonSerializerSettings SerializationSettings { get; set; } = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new Microsoft.Rest.Serialization.ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter>
            {
                new Microsoft.Rest.Serialization.Iso8601TimeSpanConverter()
            }
        };

        /// <summary>
        /// Json settings when de-serializing from strings
        /// </summary>
        public JsonSerializerSettings DeserializationSettings { get; set; } = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new Microsoft.Rest.Serialization.ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter>
            {
                new Microsoft.Rest.Serialization.Iso8601TimeSpanConverter()
            }
        };

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        // ReSharper disable once UnusedParameter.Local
        public HttpSender(string baseUri)
        {
            InternalContract.RequireNotNullOrWhiteSpace(baseUri, nameof(baseUri));try
            {
                BaseUri = new Uri(baseUri);
            }
            catch (UriFormatException e)
            {
                InternalContract.Fail($"The format of {nameof(baseUri)} ({baseUri}) is not correct: {e.Message}");
            }
            lock (LockClass)
            {
                if (HttpClient == null)
                {
                    var handlers = OutboundPipeFactory.CreateDelegatingHandlers();
                    var httpClient = HttpClientFactory.Create(handlers);
                    HttpClient = new HttpClientWrapper(httpClient);
                }
            }
            Log.LogVerbose($"Created REST client {GetType().FullName}: {baseUri}");
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        public HttpSender(string baseUri, ServiceClientCredentials credentials) : this(baseUri)
        {
            Credentials = credentials;
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        public HttpSender(string baseUri, HttpClient httpClient, ServiceClientCredentials credentials) : this(baseUri, httpClient)
        {
            Credentials = credentials;
        }


        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="httpClient">The HttpClient used when making the HTTP calls.</param>
        public HttpSender(string baseUri, HttpClient httpClient)
        {
            InternalContract.RequireNotNullOrWhiteSpace(baseUri, nameof(baseUri));
            if (!FulcrumApplication.IsInDevelopment) InternalContract.RequireNotNull(httpClient, nameof(httpClient));
            try
            {

                BaseUri = new Uri(baseUri);
            }
            catch (UriFormatException e)
            {
                InternalContract.Fail($"The format of {nameof(baseUri)} ({baseUri}) is not correct: {e.Message}");
            }

            lock (LockClass)
            {
                HttpClient = new HttpClientWrapper(httpClient);
            }
        }

        /// <summary></summary>
        /// <param name="httpSender">The HttpSender that we will base the new sender on, but change the BaseUri using the <paramref name="relativeUrl"/></param>
        /// <param name="relativeUrl">The base URL that all HTTP calls methods will refer to.</param>
        public HttpSender(IHttpSender httpSender, string relativeUrl)
        {
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            InternalContract.RequireNotNullOrWhiteSpace(relativeUrl, nameof(relativeUrl));
            try
            {
                BaseUri = new Uri(httpSender.BaseUri, relativeUrl);
            }
            catch (UriFormatException e)
            {
                InternalContract.Fail($"The format of {nameof(relativeUrl)} ({relativeUrl}) is not correct: {e.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpResponseMessage response = null;
            try
            {
                response = await SendRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken).ConfigureAwait(false);
                var request = response.RequestMessage;
                return await HandleResponseWithBody<TResponse>(method, response, request);
            }
            finally
            {
                response?.Dispose();
            }
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpRequestMessage request = null;
            try
            {
                request = await CreateRequest(method, relativeUrl, body, customHeaders, cancellationToken);
                return await SendAsync(request, cancellationToken);
            }
            finally
            {
                request?.Dispose();
            }
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpRequestMessage request = null;
            try
            {
                request = CreateRequest(method, relativeUrl, customHeaders);
                return await SendAsync(request, cancellationToken);
            }
            finally
            {
                request?.Dispose();
            }
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            FulcrumAssert.IsNotNull(response);
            return response;
        }

        #region Helpers

        private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, Dictionary<string, List<string>> customHeaders)
        {
            var baseUri = BaseUri.AbsoluteUri;
            var url = ConcatenateBaseUrlAndRelativeUrl(baseUri, relativeUrl);
            var request = new HttpRequestMessage(method, url);
            request.Headers.TryAddWithoutValidation("Accept", new List<string> {"application/json"});
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    if (request.Headers.Contains(header.Key))
                    {
                        request.Headers.Remove(header.Key);
                    }
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            return request;
        }

        private async Task<HttpRequestMessage> CreateRequest<TBody>(HttpMethod method, string relativeUrl, TBody instance, Dictionary<string, List<string>> customHeaders,
            CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var request = CreateRequest(method, relativeUrl, customHeaders);

            if (instance != null)
            {
                var requestContent = JsonConvert.SerializeObject(instance, SerializationSettings);
                request.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
                request.Content.Headers.ContentType =
                    System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            }

            if (Credentials == null) return request;

            cancellationToken.ThrowIfCancellationRequested();
            await Credentials.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
            return request;
        }

        private async Task<HttpOperationResponse<TResponse>> HandleResponseWithBody<TResponse>(HttpMethod method, HttpResponseMessage response,
            HttpRequestMessage request)
        {
            await VerifySuccessAsync(response);
            var result = new HttpOperationResponse<TResponse>
            {
                Request = request,
                Response = response,
                Body = default(TResponse)
            };

            if (method == HttpMethod.Get || method == HttpMethod.Put || method == HttpMethod.Post)
            {
                if ((method == HttpMethod.Get || method == HttpMethod.Put) && response.StatusCode != HttpStatusCode.OK)
                {
                    throw new FulcrumResourceException($"The response to request {request.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK}, but had {response.StatusCode.ToLogString()}.");
                }
                if (method == HttpMethod.Post && response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                {
                    throw new FulcrumResourceException($"The response to request {request.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK} or {HttpStatusCode.Created}, but had {response.StatusCode.ToLogString()}.");
                }
                var responseContent = await TryGetContentAsString(response.Content, false);
                if (responseContent == null) return result;
                try
                {
                    result.Body = JsonConvert.DeserializeObject<TResponse>(responseContent, DeserializationSettings);
                }
                catch (Exception e)
                {
                    throw new FulcrumResourceException($"The response to request {request.ToLogString()} could not be deserialized to the type {typeof(TResponse).FullName}. The content was:\r{responseContent}.", e);
                }
            }
            return result;
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

        private static string ConcatenateBaseUrlAndRelativeUrl(string baseUri, string relativeUrl)
        {
            var relativeUrlBeginsWithSpecialCharacter = relativeUrl.StartsWith("/") || relativeUrl.StartsWith("?");
            var slashIsRequired = !string.IsNullOrWhiteSpace(relativeUrl) && !relativeUrlBeginsWithSpecialCharacter;
            if (baseUri.EndsWith("/"))
            {
                // Maybe remove the /
                if (relativeUrlBeginsWithSpecialCharacter) baseUri = baseUri.Substring(0, baseUri.Length - 1);
            }
            else
            {
                if (slashIsRequired) baseUri += "/";
            }

            return baseUri + relativeUrl;
        }
        #endregion
    }
}
