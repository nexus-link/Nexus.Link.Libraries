using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ClientTranslators
{
    /// <inheritdoc cref="CrudClientTranslator{TModel}" />
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete since 2019-11-21.")]
    public class ManyToOneClientTranslator<TModel> : 
        ManyToOneClientTranslator<TModel, TModel>,
        ICrudManyToOne<TModel, string>
    {
        /// <inheritdoc />
        public ManyToOneClientTranslator(ICrudable<TModel, string> service,
            string parentIdConceptName, string idConceptName, System.Func<string> getClientNameMethod, ITranslatorService translatorService)
            : base(service, parentIdConceptName, idConceptName, getClientNameMethod, translatorService)
        {
        }
    }

    /// <inheritdoc cref="CrudClientTranslator{TModel}" />
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete since 2019-11-21.")]
    public class ManyToOneClientTranslator<TModelCreate, TModel> :
        CrudClientTranslator<TModelCreate, TModel>, 
        ICrudManyToOne<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrudManyToOne<TModelCreate, TModel, string> _service;
        private readonly string _parentIdConceptName;
        /// <inheritdoc />
        public ManyToOneClientTranslator(ICrudable<TModel, string> service, string parentIdConceptName, string idConceptName, System.Func<string> getClientNameMethod, ITranslatorService translatorService)
            : base(service, idConceptName, getClientNameMethod, translatorService)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(idConceptName, nameof(idConceptName));
            InternalContract.RequireNotNull(getClientNameMethod, nameof(getClientNameMethod));
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            _service = new ManyToOnePassThrough<TModelCreate, TModel, string>(service);
            _parentIdConceptName = parentIdConceptName;
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
        CancellationToken token = default)
        {
            var translator = CreateTranslator();
            parentId = translator.Decorate(_parentIdConceptName, parentId);
            var result = await _service.ReadChildrenWithPagingAsync(parentId, offset, limit, token);
            await translator.Add(result).ExecuteAsync(token);
            return translator.Translate(result);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            parentId = translator.Decorate(_parentIdConceptName, parentId);
            var result = await _service.ReadChildrenAsync(parentId, limit, token);
            var array = result as TModel[] ?? result.ToArray();
            await translator.Add(array).ExecuteAsync(token);
            return translator.Translate(array);
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(string masterId, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_parentIdConceptName, masterId);
            await _service.DeleteChildrenAsync(masterId, token);
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(string parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(string parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}