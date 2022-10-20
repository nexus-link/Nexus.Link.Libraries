using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Rest;
using Newtonsoft.Json;
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
        private static readonly HttpMethod PatchMethod = new HttpMethod("PATCH");
        /// <summary>
        /// This is the default <see cref="IHttpClient"/> that is used for all HTTP calls.
        /// </summary>
        /// <remarks>This is by default set to an HTTP client that has all outgoing pipes activated (logging, error handling, etc).
        /// Typically only set this yourself for unit test purposes.</remarks>
        public static IHttpClient DefaultHttpClient { get; set; }

        /// <summary>
        /// This is the <see cref="IHttpClient"/> that is used for all HTTP calls.
        /// </summary>
        /// <remarks>It is always initialized to a clone of <see cref="DefaultHttpClient"/>, but can be customized if needed.</remarks>
        public IHttpClient HttpClient { get; set; }

        /// <inheritdoc />
        public Uri BaseUri { get; set; }

        /// <summary>
        /// Credentials that are used when sending requests to the service.
        /// </summary>
        public ServiceClientCredentials Credentials { get; }

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
            try
            {
                if (!string.IsNullOrWhiteSpace(baseUri))
                {
                    BaseUri = new Uri(baseUri);
                }
            }
            catch (UriFormatException e)
            {
                InternalContract.Fail($"The format of {nameof(baseUri)} ({baseUri}) is not correct: {e.Message}");
            }
            lock (LockClass)
            {
                if (DefaultHttpClient == null)
                {
                    var handlers = OutboundPipeFactory.CreateDelegatingHandlers();
                    var httpClient = HttpClientFactory.Create(handlers);
                    DefaultHttpClient = new HttpClientWrapper(httpClient);
                }
            }

            HttpClient = new HttpClientWrapper(DefaultHttpClient.ActualHttpClient)
            {
                SimulateOutgoingCalls = DefaultHttpClient.SimulateOutgoingCalls
            };
            Log.LogVerbose($"Created REST client {GetType().FullName}: {baseUri}");
        }

        /// <summary></summary>
        /// <param name="baseUri">The base URL that all HTTP calls methods will refer to.</param>
        /// <param name="credentials">The credentials used when making the HTTP calls.</param>
        public HttpSender(string baseUri, ServiceClientCredentials credentials) : this(baseUri)
        {
            Credentials = credentials;
        }

        /// <inheritdoc />
        public IHttpSender CreateHttpSender(string relativeUrl)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));

            var newUri = GetAbsoluteUrl(relativeUrl);
            return new HttpSender(newUri, Credentials)
            {
                HttpClient = HttpClient
            };
        }

        #region TResponse
        /// <inheritdoc />
        public virtual async Task<TResponse> SendRequestThrowIfNotSuccessAsync<TResponse, TBody>(
            HttpMethod method,
            string relativeUrl,
            TBody body = default,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync<TResponse, TBody>(method, relativeUrl, body, customHeaders, cancellationToken);
            var result = await VerifySuccessAndReturnBodyAsync(response, cancellationToken);
            return result;
        }

        /// <inheritdoc />
        public virtual async Task SendRequestThrowIfNotSuccessAsync<TBody>(
            HttpMethod method,
            string relativeUrl,
            TBody body = default,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            var response = await SendRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken);
            await VerifySuccessAsync(response, cancellationToken);
        }
        #endregion

        #region HttpOperationResponse
        /// <inheritdoc />
        public virtual async Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var response = await SendRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken).ConfigureAwait(false);
            return await HandleResponseWithBody(response);

            async Task<HttpOperationResponse<TResponse>> HandleResponseWithBody(HttpResponseMessage responseMessage)
            {
                var requestMessage = responseMessage.RequestMessage;
                var result = new HttpOperationResponse<TResponse>
                {
                    Request = requestMessage,
                    Response = responseMessage,
                    Body = default
                };

                if (!responseMessage.IsSuccessStatusCode) return result;

                // Simple case
                if (responseMessage.StatusCode == HttpStatusCode.NoContent) return result;

                // Se if we should try to extract a content from the responseMessage;
                if (method == HttpMethod.Get || method == HttpMethod.Put || method == HttpMethod.Post || method == PatchMethod)
                {
                    if ((method == HttpMethod.Get || method == HttpMethod.Put || method == PatchMethod) && responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        throw new FulcrumResourceException($"The responseMessage to requestMessage {requestMessage.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK}, but had {responseMessage.StatusCode.ToLogString()}.");
                    }

                    if (method == HttpMethod.Post && responseMessage.StatusCode != HttpStatusCode.OK && responseMessage.StatusCode != HttpStatusCode.Created)
                    {
                        throw new FulcrumResourceException($"The responseMessage to requestMessage {requestMessage.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK} or {HttpStatusCode.Created}, but had {responseMessage.StatusCode.ToLogString()}.");
                    }
                    var responseContent = await TryGetContentAsStringAsync(responseMessage.Content, false, cancellationToken);
                    if (responseContent == null) return result;
                    try
                    {
                        result.Body = JsonConvert.DeserializeObject<TResponse>(responseContent, DeserializationSettings);
                    }
                    catch (Exception e)
                    {
                        throw new FulcrumResourceException($"The responseMessage to requestMessage {requestMessage.ToLogString()} could not be deserialized to the type {typeof(TResponse).FullName}. The content was:\r{responseContent}.", e);
                    }
                }
                return result;
            }
        }

        /// <inheritdoc />
        public virtual async Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = null;
            try
            {
                request = await CreateRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken);
                return await SendAsync(request, cancellationToken);
            }
            finally
            {
                request?.Dispose();
            }
        }
        #endregion

        #region HttpResponseMessage
        /// <inheritdoc />
        public virtual async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = null;
            try
            {
                request = await CreateRequestAsync(method, relativeUrl, customHeaders);
                return await SendAsync(request, cancellationToken);
            }
            finally
            {
                request?.Dispose();
            }
        }

        /// <inheritdoc />
        public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            FulcrumAssert.IsNotNull(response);
            return response;
        }
        #endregion

        #region Helpers

        public virtual string GetAbsoluteUrl(string relativeUrl)
        {
            string baseUri = BaseUri?.OriginalString ?? HttpClient.ActualHttpClient?.BaseAddress?.OriginalString ?? "";

            if (baseUri != "" && !string.IsNullOrWhiteSpace(relativeUrl))
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
            }

            var concatenatedUrl = baseUri + relativeUrl?.Trim(' ');

            if (!(Uri.TryCreate(concatenatedUrl, UriKind.Absolute, out var uri)
                  && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)))
            {
                InternalContract.Fail($"The format of the concatenated url ({concatenatedUrl}) is not correct. BaseUrl: '{baseUri}'. RelativeUrl: '{relativeUrl}'");
            }

            return concatenatedUrl;
        }

        protected virtual async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string relativeUrl, Dictionary<string, List<string>> customHeaders)
        {
            var url = GetAbsoluteUrl(relativeUrl);
            var request = new HttpRequestMessage(method, url);
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (!request.Headers.Contains("Accept"))
            {
                request.Headers.TryAddWithoutValidation("Accept", new List<string> { "application/json" });
            }

            if (Credentials == null) return request;

            await Credentials.ProcessHttpRequestAsync(request, default).ConfigureAwait(false);
            return request;
        }

        public static async Task<string> TryGetContentAsStringAsync(HttpContent content, bool silentlyIgnoreExceptions, CancellationToken cancellationToken)
        {
            if (content == null) return null;
            try
            {
                await content.LoadIntoBufferAsync();
                return await content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!silentlyIgnoreExceptions) throw new FulcrumAssertionFailedException("Expected to be able to read an HttpContent.", e);
            }
            return null;
        }

        protected virtual async Task<HttpRequestMessage> CreateRequestAsync<TBody>(HttpMethod method, string relativeUrl, TBody instance, Dictionary<string, List<string>> customHeaders,
            CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));

            var request = await CreateRequestAsync(method, relativeUrl, customHeaders);
            if (instance == null) return request;

            var requestContent = JsonConvert.SerializeObject(instance, SerializationSettings);
            request.Content = new StringContent(requestContent, System.Text.Encoding.UTF8);
            request.Content.Headers.ContentType =
                System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            return request;
        }
        #endregion

        /// <summary>
        /// Use this method for cases where you always expect the <paramref name="operationResponse"/> to be successful.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The Body of <paramref name="operationResponse"/></returns>
        public static async Task<T> VerifySuccessAndReturnBodyAsync<T>(HttpOperationResponse<T> operationResponse,
            CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(operationResponse, nameof(operationResponse));
            InternalContract.RequireNotNull(operationResponse.Response,
                $"{nameof(operationResponse)}.{nameof(operationResponse.Response)}");


            if ((int)operationResponse.Response.StatusCode >= 300 && (int)operationResponse.Response.StatusCode < 400)
            {
                // We have a redirect. If we can interpret the old and new id, we could throw a redirect exception
                if (TryInterpretRedirectException(operationResponse.Response, out var redirectException)) throw redirectException;
            }
            await VerifySuccessAsync(operationResponse.Response, cancellationToken);
            return operationResponse.Body;
        }

        /// <summary>
        /// Use this method for cases where you always expect the <paramref name="response"/> to be successful.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The Body of <paramref name="response"/></returns>
        public static async Task VerifySuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            InternalContract.RequireNotNull(response.RequestMessage, $"{nameof(response)}.{nameof(response.RequestMessage)}");

            if (response.IsSuccessStatusCode) return;

            var requestContent =
                await TryGetContentAsStringAsync(response.RequestMessage?.Content, true, cancellationToken);
            var responseContent =
                await TryGetContentAsStringAsync(response.Content, true, cancellationToken);
            var message = $"{response.StatusCode} {responseContent}";
            var exception = new HttpOperationException(message)
            {
                Response = new HttpResponseMessageWrapper(response, responseContent),
                Request = new HttpRequestMessageWrapper(response.RequestMessage, requestContent)
            };
            throw exception;
        }

        private static bool TryInterpretRedirectException(HttpResponseMessage response, out Exception exception)
        {
            exception = null;
            if (response.Headers.Location == null) return false;
            if (response.RequestMessage.RequestUri == null) return false;
            if (response.RequestMessage.RequestUri.Host != response.Headers.Location.Host) return false;
            var requestPath = WebUtility.UrlDecode(response.RequestMessage.RequestUri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(requestPath)) return false;
            var redirectPath = WebUtility.UrlDecode(response.Headers.Location.AbsolutePath);
            if (string.IsNullOrWhiteSpace(redirectPath)) return false;
            var differsAt = DiffersAtIndex(requestPath, redirectPath);
            // Same or totally different
            if (differsAt < 1) return false;
            var oldIdWithTail = RemoveHeadToMax(requestPath, differsAt);
            var newIdWithTail = RemoveHeadToMax(redirectPath, differsAt);
            var oldId = RemoveTail(oldIdWithTail);
            if (string.IsNullOrWhiteSpace(oldId)) return false;
            var newId = RemoveTail(newIdWithTail);
            if (string.IsNullOrWhiteSpace(newId)) return false;
            var oldTail = oldIdWithTail.Substring(oldId.Length);
            var newTail = newIdWithTail.Substring(newId.Length);
            if (oldTail != newTail) return false;
            exception = new FulcrumRedirectException(oldId, newId);
            return true;
            
            int DiffersAtIndex(string s1, string s2)
            {
                int index = 0;
                int min = Math.Min(s1.Length, s2.Length);
                while (index < min && s1[index] == s2[index])
                    index++;

                return (index == min && s1.Length == s2.Length) ? -1 : index;
            }

            string RemoveHeadToMax(string s, int max)
            {
                for (int i = max; i >=0; i--)
                {
                    var c = s[i];
                    if (c == '/' || c == '?')
                    {
                        return s.Substring(i+1, s.Length-i-1);
                    }
                }
                return s;
            }

            string RemoveTail(string s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    var c = s[i];
                    if (c == '/' || c == '?') return s.Substring(0, i);
                }
                return s;
            }
        }
    }
}
