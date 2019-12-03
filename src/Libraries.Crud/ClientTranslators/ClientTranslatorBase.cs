using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Translation;

namespace Nexus.Link.Libraries.Crud.ClientTranslators
{
    /// <summary>
    /// Decorate values from client and translate concept values to client.
    /// </summary>
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete since 2019-11-21.")]
    public abstract class ClientTranslatorBase
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
        /// The name of the client. Is used for decorating values and for translating back to client.
        /// </summary>
        protected System.Func<string> GetClientNameMethod { get; }

        /// <summary>
        /// Set up a new client translator.
        /// </summary>
        /// <param name="idConceptName">The <see cref="IdConceptName"/>.</param>
        /// <param name="getClientNameMethod">The <see cref="GetClientNameMethod"/>.</param>
        /// <param name="translatorService">The <see cref="TranslatorService"/>.</param>
        protected ClientTranslatorBase(string idConceptName, System.Func<string> getClientNameMethod, ITranslatorService translatorService)
        {
            InternalContract.RequireNotNullOrWhiteSpace(idConceptName, nameof(idConceptName));
            InternalContract.RequireNotNull(getClientNameMethod, nameof(getClientNameMethod));
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            IdConceptName = idConceptName;
            GetClientNameMethod = getClientNameMethod;
            TranslatorService = translatorService;
            _translatorFactory = new TranslatorFactory(TranslatorService, GetClientNameMethod);
        }

        /// <summary>
        /// Returns a new translator for a client. 
        /// </summary>
        protected ITranslator CreateTranslator()
        {
            return _translatorFactory.CreateTranslator();
        }
    }
}