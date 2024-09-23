using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Translates outgoing requests (including URI and Content)
    /// </summary>
    public class ValueTranslationHandler : DelegatingHandler
    {
        private readonly ITranslatorService _translatorService;
        private readonly string _translationTargetClientName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="translatorService">The service to use for translations</param>
        /// <param name="translationTargetClientName">The target client name for translations.</param>
        public ValueTranslationHandler(string translationTargetClientName, ITranslatorService translatorService = null)
        {
            InternalContract.RequireNotNullOrWhiteSpace(translationTargetClientName, nameof(translationTargetClientName));
            
            _translationTargetClientName = translationTargetClientName;
            _translatorService = translatorService;
        }

        /// <summary>
        /// Adds a Fulcrum CorrelationId to the requests before sending it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_translatorService == null) return await base.SendAsync(request, cancellationToken);

            var translator = new Translator(_translationTargetClientName, _translatorService);

            // Prepare translation of user ids from the context
            var userIds = FulcrumApplication.Context.ValueProvider.GetValue<List<string>>("DecoratedUserIds");
            if (userIds != null && userIds.Any())
            {
                foreach (var decoratedUserId in userIds)
                {
                    translator.Add(decoratedUserId);
                }
            }

            // Prepare translation of the RequestUri
            var originalUri = request.RequestUri?.ToString();
            if (!string.IsNullOrWhiteSpace(originalUri))
            {
                translator.AddSubStrings(originalUri);
            }

            // Prepare translation of the content
            var originalContent = request.Content == null
                ? null
                : await request.Content.ReadAsStringAsync();
            var mediaType = "application/json";
            if (!string.IsNullOrWhiteSpace(originalContent))
            {
                mediaType = request.Content!.Headers.ContentType?.MediaType ?? "application/json";
                translator.AddSubStrings(originalContent);
            }

            // Translate all the individual values
            await translator.ExecuteAsync(cancellationToken);

            // Set the context user ids to the translated values
            if (userIds != null && userIds.Any())
            {
                SetupTranslatedUserId(translator, userIds);
            }

            // Translate the request URI
            if (!string.IsNullOrWhiteSpace(originalUri))
            {
                // Translate the URI
                var newUri = translator.Translate(originalUri);
                request.RequestUri = new Uri(newUri);
            }

            // Translate the request Content
            if (!string.IsNullOrWhiteSpace(originalContent))
            {
                // Translate the URI
                var newContent = translator.Translate(originalContent);
                request.Content = new StringContent(newContent, Encoding.UTF8, mediaType);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private void SetupTranslatedUserId(ITranslator translator, IEnumerable<string> userIds)
        {
            string translatedUserId = null;
            foreach (var translated in userIds
                         .Select(translator.Translate)
                         .Where(translated => !translated.Contains("!~")))
            {
                translatedUserId = translated;
            }
            
            TranslatedUserId_OnlyForUnitTests = FulcrumApplication.Context.ValueProvider.GetValue<string>(Constants.TranslatedUserIdKey);
            if (translatedUserId == null) return;
            FulcrumApplication.Context.ValueProvider.SetValue(Constants.TranslatedUserIdKey, translator.Translate(translatedUserId));
        }

        // ReSharper disable once InconsistentNaming
        public string TranslatedUserId_OnlyForUnitTests { get; private set; }
    }
}
