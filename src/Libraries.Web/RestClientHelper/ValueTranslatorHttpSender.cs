﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.RestClientHelper
{
    /// <summary>
    /// Convenience client for making REST calls
    /// </summary>
    public class ValueTranslatorHttpSender : IHttpSender, ITranslationClientName
    {
        /// <summary>
        /// The service to use for translation
        /// </summary>
        public static ITranslatorService TranslatorService { get; set; }

        public IHttpSender HttpSender { get; }

        /// <inheritdoc />
        public string TranslationClientName { get; }

        /// <inheritdoc />
        public Uri BaseUri => HttpSender.BaseUri;

        public ValueTranslatorHttpSender(IHttpSender httpSender, string translationClientName)
        {
            InternalContract.RequireNotNull(httpSender, nameof(httpSender));
            InternalContract.RequireNotNullOrWhiteSpace(translationClientName, nameof(translationClientName));
            HttpSender = httpSender;

            TranslationClientName = translationClientName;
        }

        /// <inheritdoc />
        public IHttpSender CreateHttpSender(string relativeUrl)
        {
            InternalContract.RequireNotNull(relativeUrl, nameof(relativeUrl));
            return new ValueTranslatorHttpSender(HttpSender.CreateHttpSender(relativeUrl), TranslationClientName);
        }

        /// <inheritdoc />
        public async Task<HttpOperationResponse<TResponse>> SendRequestAsync<TResponse, TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var userIds = FulcrumApplication.Context.ValueProvider.GetValue<List<string>>("DecoratedUserIds");

            var translator = CreateTranslator();
            translator.AddSubStrings(relativeUrl).Add(body);

            if (userIds != null && userIds.Any())
            {
                foreach (var decoratedUserId in userIds)
                {
                    translator.Add(decoratedUserId);
                }
            }

            await translator.ExecuteAsync(cancellationToken);

            SetupTranslatedUserId(translator, userIds);

            var result = await HttpSender.SendRequestAsync<TResponse, TBody>(
                method,
                translator.Translate(relativeUrl),
                translator.Translate(body),
                customHeaders,
                cancellationToken);
            result.Body = (TResponse)translator.Decorate(result.Body, typeof(TResponse));
            return result;
        }

        /// <inheritdoc />
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return HttpSender.SendAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public string GetAbsoluteUrl(string relativeUrl)
        {
            return HttpSender.GetAbsoluteUrl(relativeUrl);
        }

        /// <inheritdoc />
        public async Task<HttpResponseMessage> SendRequestAsync<TBody>(HttpMethod method, string relativeUrl,
            TBody body = default, Dictionary<string, List<string>> customHeaders = null,
            CancellationToken cancellationToken = default)
        {
            var userIds = FulcrumApplication.Context.ValueProvider.GetValue<List<string>>("DecoratedUserIds");

            var translator = CreateTranslator();
            translator.AddSubStrings(relativeUrl).Add(body);

            if (userIds != null && userIds.Any())
            {
                foreach (var decoratedUserId in userIds)
                {
                    translator.Add(decoratedUserId);
                }
            }

            await translator.ExecuteAsync(cancellationToken);

            SetupTranslatedUserId(translator, userIds);

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
            Dictionary<string, List<string>> customHeaders = null, CancellationToken cancellationToken = default)
        {
            var userIds = FulcrumApplication.Context.ValueProvider.GetValue<List<string>>("DecoratedUserIds");

            var translator = CreateTranslator();
            if (userIds != null && userIds.Any())
            {
                foreach (var decoratedUserId in userIds)
                {
                    translator.Add(decoratedUserId);
                }
            }

            await translator.ExecuteAsync(cancellationToken);
            SetupTranslatedUserId(translator, userIds);

            return await HttpSender.SendRequestAsync(method, translator.Translate(relativeUrl), customHeaders, cancellationToken);
        }

        private void SetupTranslatedUserId(ITranslator translator, List<string> userIds)
        {
            var translatedUserId = userIds.FirstOrDefault(userId => !userId.Contains("!~"));

            if (translatedUserId != null)
            {
                FulcrumApplication.Context.ValueProvider.SetValue(Constants.TranslatedUserIdKey, translator.Translate(userId));
                TranslatedUserId_OnlyForUnitTests = FulcrumApplication.Context.ValueProvider.GetValue<string>(Constants.TranslatedUserIdKey);
            }
        }

        // ReSharper disable once InconsistentNaming
        public string TranslatedUserId_OnlyForUnitTests { get; private set; }

        private ITranslator CreateTranslator()
        {
            InternalContract.Require(TranslatorService != null,
                $"{nameof(ValueTranslatorHttpSender)}.{nameof(TranslatorService)} must be set. It is a static property, so you normally set it once when your app starts up.");
            return new Translator(TranslationClientName, TranslatorService);
        }
    }
}
