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
    public class ValueTranslatorHttpSender : IValueTranslatorHttpSender
    {
        private readonly TranslatorFactory _translatorFactory;
        public IHttpSender HttpSender { get; }

        /// <inheritdoc />
        public Uri BaseUri => HttpSender.BaseUri;

        public ValueTranslatorHttpSender(IHttpSender httpSender, TranslatorFactory translatorFactory)
        {
            _translatorFactory = translatorFactory;
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            InternalContract.RequireNotNull(translatorFactory, nameof(translatorFactory));
            InternalContract.RequireValidated(translatorFactory, nameof(translatorFactory));
            HttpSender = httpSender;
        }

        /// <inheritdoc />
        public async Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: Add translation and decoration
            var translator = _translatorFactory.CreateTranslator();
            await translator.AddSubStrings(relativeUrl).Add(body).ExecuteAsync(cancellationToken);
            var result = await HttpSender.SendRequestAsync<TResponse, TBody>(
                method,
                translator.Translate(relativeUrl),
                translator.Translate(body), 
                customHeaders,
                cancellationToken);
            result.Body = translator.DecorateItem(result.Body);
            return result;
        }

        public async Task<HttpOperationResponse<string>> SendRequestAndDecorateResponseAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            InternalContract.Require(!string.IsNullOrWhiteSpace(_translatorFactory.DefaultConceptName), 
                $"You must have set the {nameof(_translatorFactory.DefaultConceptName)} in {nameof(TranslatorFactory)} to use this method.");
            var translator = _translatorFactory.CreateTranslator();
            await translator.AddSubStrings(relativeUrl).Add(body).ExecuteAsync(cancellationToken);
            var result = await HttpSender.SendRequestAsync<string, TBody>(
                method,
                translator.Translate(relativeUrl),
                translator.Translate(body), 
                customHeaders,
                cancellationToken);
            result.Body = translator.DecorateWithDefaultConceptName(result.Body);
            return result;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var translator = _translatorFactory.CreateTranslator();
            await translator.Add(relativeUrl).Add(body).ExecuteAsync(cancellationToken);
            var result = await HttpSender.SendRequestAsync(
                method,
                translator.Translate(relativeUrl),
                translator.Translate(body), 
                customHeaders,
                cancellationToken);
            return result;
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUrl,
            Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var translator = _translatorFactory.CreateTranslator();
            await translator.AddSubStrings(relativeUrl).ExecuteAsync(cancellationToken);
            return await HttpSender.SendRequestAsync(method, translator.Translate(relativeUrl), customHeaders, cancellationToken);
        }
    }
}
