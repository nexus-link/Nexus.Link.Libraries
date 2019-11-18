using System;
using System.Collections.Generic;
using System.Text;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Translation
{
    /// <summary>
    /// The information needed to do translation for a client
    /// </summary>
    public class TranslatorFactory : IValidatable
    {
        private readonly Func<string> _getClientNameMethod;
        private readonly string _clientName;

        /// <summary>
        /// A factory with a fixed client name
        /// </summary>
        public TranslatorFactory(ITranslatorService translatorService, string clientName)
        {
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            InternalContract.RequireNotNullOrWhiteSpace(clientName, nameof(clientName));
            _clientName = clientName;
            TranslatorService = translatorService;
        }

        /// <summary>
        /// A factory with a dynamic client name. The client name is set at the creation of the translator instance.
        /// </summary>
        public TranslatorFactory(ITranslatorService translatorService, Func<string> getClientNameMethod)
        {
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            InternalContract.RequireNotNull(getClientNameMethod, nameof(getClientNameMethod));
            _getClientNameMethod = getClientNameMethod;
            TranslatorService = translatorService;
        }
        /// <summary>
        /// The service that does the actual translation.
        /// </summary>
        public ITranslatorService TranslatorService { get; set; }

        public Translator CreateTranslator()
        {
            return new Translator(ClientName, TranslatorService, DefaultConceptName);
        }

        /// <summary>
        /// The client name.
        /// </summary>
        public string ClientName => _clientName ?? _getClientNameMethod();

        public string DefaultConceptName { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(TranslatorService, nameof(TranslatorService), errorLocation);
            FulcrumValidate.IsTrue(_clientName != null || _getClientNameMethod != null, errorLocation,
                $"It is not possible to find the client name for the translations.");
        }
    }
}
