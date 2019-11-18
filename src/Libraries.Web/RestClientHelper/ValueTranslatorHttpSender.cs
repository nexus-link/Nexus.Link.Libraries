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
        private readonly TranslatorSetup _translatorSetup;
        public IHttpSender HttpSender { get; }

        /// <inheritdoc />
        public Uri BaseUri => HttpSender.BaseUri;

        /// <inheritdoc />
        public ServiceClientCredentials Credentials => HttpSender.Credentials;

        public ValueTranslatorHttpSender(IHttpSender httpSender, TranslatorSetup translatorSetup)
        {
            _translatorSetup = translatorSetup;
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            InternalContract.RequireNotNull(translatorSetup, nameof(translatorSetup));
            InternalContract.RequireValidated(translatorSetup, nameof(translatorSetup));
            HttpSender = httpSender;
        }

        /// <inheritdoc />
        public async Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default(TBody), Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: Add translation and decoration
            var translator = new Translator(_translatorSetup);
            await translator.Add(relativeUrl).Add(body).ExecuteAsync();
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
            InternalContract.Require(!string.IsNullOrWhiteSpace(_translatorSetup.DefaultConceptName), 
                $"You must have set the {nameof(_translatorSetup.DefaultConceptName)} in translator setup to use this method.");
            var translator = new Translator(_translatorSetup);
            await translator.Add(relativeUrl).Add(body).ExecuteAsync();
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
            var translator = new Translator(_translatorSetup);
            await translator.Add(relativeUrl).Add(body).ExecuteAsync();
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
            var translator = new Translator(_translatorSetup);
            await translator.Add(relativeUrl).ExecuteAsync();
            return await HttpSender.SendRequestAsync(method, translator.Translate(relativeUrl), customHeaders, cancellationToken);
        }
    }
}
