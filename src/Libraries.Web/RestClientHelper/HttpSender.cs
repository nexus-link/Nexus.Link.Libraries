using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        /// To what depth will we follow redirects? 0 means don't follow redirects at all.
        /// </summary>
        public int OptionFollowRedirectsToDepth { get; set; }

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
        public async Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await SendRequestAsync(method, relativeUrl, body, customHeaders, cancellationToken).ConfigureAwait(false);
                return await HandleResponseWithBody(response);
            }
            finally
            {
                response?.Dispose();
            }


            async Task<HttpOperationResponse<TResponse>> HandleResponseWithBody(HttpResponseMessage httpResponseMessage)
            {
                var result = new HttpOperationResponse<TResponse>
                {
                    Request = httpResponseMessage.RequestMessage,
                    Response = httpResponseMessage,
                    Body = default
                };

                // Simple case
                if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent) return result;

                await VerifySuccessAsync(httpResponseMessage, cancellationToken);
                if (method == HttpMethod.Get || method == HttpMethod.Put || method == HttpMethod.Post || method == PatchMethod)
                {
                    if ((method == HttpMethod.Get || method == HttpMethod.Put || method == PatchMethod) && httpResponseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        throw new FulcrumResourceException($"The response to request {httpResponseMessage.RequestMessage.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK}, but had {response.StatusCode.ToLogString()}.");
                    }

                    if (method == HttpMethod.Post && httpResponseMessage.StatusCode != HttpStatusCode.OK && httpResponseMessage.StatusCode != HttpStatusCode.Created)
                    {
                        throw new FulcrumResourceException($"The response to request {httpResponseMessage.RequestMessage.ToLogString()} was expected to have HttpStatusCode {HttpStatusCode.OK} or {HttpStatusCode.Created}, but had {response.StatusCode.ToLogString()}.");
                    }
                    var responseContent = await TryGetContentAsStringAsync(httpResponseMessage.Content, false, cancellationToken);
                    if (responseContent == null) return result;
                    try
                    {
                        result.Body = JsonConvert.DeserializeObject<TResponse>(responseContent, DeserializationSettings);
                    }
                    catch (Exception e)
                    {
                        throw new FulcrumResourceException($"The response to request {httpResponseMessage.RequestMessage.ToLogString()} could not be deserialized to the type {typeof(TResponse).FullName}. The content was:\r{responseContent}.", e);
                    }
                }
                return result;
            }
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
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

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl,
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
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var urls = new HashSet<string>();
            do
            {
                var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                FulcrumAssert.IsNotNull(response);
                cancellationToken.ThrowIfCancellationRequested();
                var statusCodeAsInt = (int) response.StatusCode;
                if (statusCodeAsInt < 300 || statusCodeAsInt >= 400) return response;
                    
                if (OptionFollowRedirectsToDepth < 1)
                {
                    throw new FulcrumResourceException(
                        $"The response for {request} had HTTP status code {statusCodeAsInt} (redirect)," +
                        $" but {GetType().FullName} had {nameof(OptionFollowRedirectsToDepth)} set to .{OptionFollowRedirectsToDepth}");
                }
                var found = response.Headers.TryGetValues("Location", out var locations);
                string[] locationsArray = null;
                if (found)
                {
                    locationsArray = locations.ToArray();
                }

                if (!found || !locationsArray.Any())
                {
                    throw new FulcrumResourceException(
                        $"The response for {request} had HTTP status code {statusCodeAsInt} (redirect), but it had no Location URL header.");
                }

                if (locationsArray.Length > 1)
                {
                    throw new FulcrumNotImplementedException(
                        $"The response for {request} was a redirect. The Location header had more than one value ({string.Join(", ", locationsArray)}) and that is currently not supported by {GetType().FullName}.{nameof(SendAsync)}()");
                }

                var location = locationsArray[0];
                if (string.IsNullOrEmpty(location))
                {
                    throw new FulcrumResourceException(
                        $"The response for {request} had HTTP status code {statusCodeAsInt} (redirect), but it the Location URL header was empty.");
                }

                if (urls.Contains(location))
                {
                    throw new FulcrumResourceException(
                        $"The response for {request} had a circular redirect: {string.Join(", ", urls)}.");
                }

                urls.Add(location);
                request.RequestUri = new Uri(location);
            } while (urls.Count <= OptionFollowRedirectsToDepth);
            throw new FulcrumResourceException($"The response for {request} had a more than {OptionFollowRedirectsToDepth} redirects: {string.Join(", ", urls)}.");
        }

        #region Helpers

        protected async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string relativeUrl, Dictionary<string, List<string>> customHeaders)
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

        protected async Task<HttpRequestMessage> CreateRequestAsync<TBody>(HttpMethod method, string relativeUrl, TBody instance, Dictionary<string, List<string>> customHeaders,
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

        private async Task VerifySuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            InternalContract.RequireNotNull(response.RequestMessage, $"{nameof(response)}.{nameof(response.RequestMessage)}");
            if (!response.IsSuccessStatusCode)
            {
                var requestContent = await TryGetContentAsStringAsync(response.RequestMessage?.Content, true, cancellationToken);
                var responseContent = await TryGetContentAsStringAsync(response.Content, true, cancellationToken);
                var message = $"{response.StatusCode} {responseContent}";
                var exception = new HttpOperationException(message)
                {
                    Response = new HttpResponseMessageWrapper(response, responseContent),
                    Request = new HttpRequestMessageWrapper(response.RequestMessage, requestContent)
                };
                throw exception;
            }
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

        public string GetAbsoluteUrl(string relativeUrl)
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
        #endregion
    }
}
