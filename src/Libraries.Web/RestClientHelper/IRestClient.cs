﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <summary>
    /// Support for calling a REST service.
    /// </summary>
    public interface IRestClient : IHttpSender
    {

        #region POST

        /// <summary>
        /// Send POST to <paramref name="relativeUrl"/> with <paramref name="body"/> and with a response of another type than the type of <paramref name="body"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type for the response.</typeparam>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The POST body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An object.</returns>
        Task<TResponse> PostAsync<TResponse, TBody>(
            string relativeUrl,
            TBody body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send POST to <paramref name="relativeUrl"/> with no body, but with a returned object.
        /// </summary>
        /// <typeparam name="TResponse">The type for the returned result.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An object.</returns>
        Task<TResponse> PostAsync<TResponse>(
            string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send POST to <paramref name="relativeUrl"/> with <paramref name="body"/> with the created object as the returned value.
        /// </summary>
        /// <typeparam name="TBodyAndResponse">The type for both the <paramref name="body"/> and the returned result.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The POST body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The created object of the same type as the <paramref name="body"/>.</returns>
        Task<TBodyAndResponse> PostAndReturnCreatedObjectAsync<TBodyAndResponse>(
            string relativeUrl,
            TBodyAndResponse body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send POST to <paramref name="relativeUrl"/> with <paramref name="body"/> with no returned value.
        /// </summary>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The POST body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task PostNoResponseContentAsync<TBody>(
            string relativeUrl,
            TBody body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Post to <paramref name="relativeUrl"/> with no body and no returned value.
        /// </summary>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task PostNoResponseContentAsync(string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);
        #endregion

        #region GET

        /// <summary>
        /// Send GET to <paramref name="relativeUrl"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type of the returned object.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The found object.</returns>
        Task<TResponse> GetAsync<TResponse>(
            string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);
        #endregion

        #region PUT

        /// <summary>
        /// Send PUT to <paramref name="relativeUrl"/> with <paramref name="body"/> and with a response of another type than the type of <paramref name="body"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type for the response.</typeparam>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An object.</returns>
        Task<TResponse> PutAsync<TResponse, TBody>(
            string relativeUrl, TBody body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send PUT to <paramref name="relativeUrl"/> with <paramref name="body"/> with the updated object as the returned value.
        /// </summary>
        /// <typeparam name="TBodyAndResponse">The type for both the <paramref name="body"/> and the returned result.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated object with the same type as the <paramref name="body"/>.</returns>
        Task<TBodyAndResponse> PutAndReturnUpdatedObjectAsync<TBodyAndResponse>(
            string relativeUrl,
            TBodyAndResponse body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send PUT to <paramref name="relativeUrl"/> with <paramref name="body"/> with no returned value.
        /// </summary>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task PutNoResponseContentAsync<TBody>(
            string relativeUrl,
            TBody body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);
        #endregion

        #region PATCH

        /// <summary>
        /// Send PUT to <paramref name="relativeUrl"/> with <paramref name="body"/> and with a response of another type than the type of <paramref name="body"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type for the response.</typeparam>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An object.</returns>
        Task<TResponse> PatchAsync<TResponse, TBody>(
            string relativeUrl, TBody body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send PUT to <paramref name="relativeUrl"/> with <paramref name="body"/> with the updated object as the returned value.
        /// </summary>
        /// <typeparam name="TBodyAndResponse">The type for both the <paramref name="body"/> and the returned result.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The updated object with the same type as the <paramref name="body"/>.</returns>
        Task<TBodyAndResponse> PatchAndReturnUpdatedObjectAsync<TBodyAndResponse>(
            string relativeUrl,
            TBodyAndResponse body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send PUT to <paramref name="relativeUrl"/> with <paramref name="body"/> with no returned value.
        /// </summary>
        /// <typeparam name="TBody">The type for the <paramref name="body"/>.</typeparam>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="body">The PUT body.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task PatchNoResponseContentAsync<TBody>(
            string relativeUrl,
            TBody body,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);
        #endregion

        #region DELETE

        /// <summary>
        /// Send DELETE to <paramref name="relativeUrl"/>.
        /// </summary>
        /// <param name="relativeUrl">The Url relative to <see cref="IHttpSender.BaseUri"/>, including parameters, etc.</param>
        /// <param name="customHeaders">Optional headers.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        Task DeleteAsync(string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default);
        #endregion
    }
}
