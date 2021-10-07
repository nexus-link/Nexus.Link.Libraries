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
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
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
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
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
        CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            parentId = translator.Decorate(_parentIdConceptName, parentId);
            var result = await _service.ReadChildrenWithPagingAsync(parentId, offset, limit, cancellationToken );
            await translator.Add(result).ExecuteAsync(cancellationToken );
            return translator.Translate(result);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            parentId = translator.Decorate(_parentIdConceptName, parentId);
            var result = await _service.ReadChildrenAsync(parentId, limit, cancellationToken );
            var array = result as TModel[] ?? result.ToArray();
            await translator.Add(array).ExecuteAsync(cancellationToken );
            return translator.Translate(array);
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(string masterId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_parentIdConceptName, masterId);
            await _service.DeleteChildrenAsync(masterId, cancellationToken );
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

        /// <inheritdoc />
        public Task<string> CreateChildAsync(string parentId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> CreateChildAndReturnAsync(string parentId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task CreateChildWithSpecifiedIdAsync(string parentId, string childId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> CreateChildWithSpecifiedIdAndReturnAsync(string parentId, string childId, TModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }
    }
}