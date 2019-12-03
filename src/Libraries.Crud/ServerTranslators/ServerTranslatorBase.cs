using System;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Crud.ServerTranslators
{
    /// <summary>
    /// Decorate values from server and translate concept values to server.
    /// </summary>
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete since 2019-11-21.")]
    public abstract class ServerTranslatorBase
    {
        private readonly ITranslatorFactory _translatorFactory;

        /// <summary>
        /// The concept name for the id. Is used for translations of id parameters and id results.
        /// </summary>
        protected string IdConceptName { get; }

        /// <summary>
        /// The service that should carry out the actual translation of values decorated into concept values.
        /// </summary>
        protected ITranslatorService TranslatorService { get; }

        // TODO: Read client from context
        /// <summary>
        /// The name of the server. Is used for decorating values from the server and for translating to the server.
        /// </summary>
        protected System.Func<string> GetServerNameMethod { get; }

        /// <summary>
        /// Set up a new client translator.
        /// </summary>
        /// <param name="idConceptName">The <see cref="IdConceptName"/>.</param>
        /// <param name="getServerNameMethod">The <see cref="GetServerNameMethod"/>.</param>
        /// <param name="translatorService">The <see cref="TranslatorService"/>. Originally expected to be null for translators from the server, but FakeTranslatorService was added for these obsolete classes.</param>
        protected ServerTranslatorBase(string idConceptName, System.Func<string> getServerNameMethod, ITranslatorService translatorService)
        {
            IdConceptName = idConceptName;
            GetServerNameMethod = getServerNameMethod;
            TranslatorService = translatorService;
            _translatorFactory = new TranslatorFactory(TranslatorService, GetServerNameMethod);
        }

        /// <summary>
        /// Returns a new translator for a server. 
        /// </summary>
        protected ITranslator CreateTranslator()
        {
            return _translatorFactory.CreateTranslator();
        }
    }
}