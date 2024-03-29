﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ServerTranslators.To
{
    /// <inheritdoc cref="ManyToOneToServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class ManyToOneToServerTranslator<TModel> : 
        ManyToOneToServerTranslator<TModel, TModel>, 
        ICrudManyToOne<TModel, string>
    {

        /// <inheritdoc />
        public ManyToOneToServerTranslator(ICrudable<TModel, string> service, string idConceptName,
            System.Func<string> getServerNameMethod, ITranslatorService translatorService)
            : base(service, idConceptName, getServerNameMethod, translatorService)
        {
        }
    }

    /// <inheritdoc cref="CrudToServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class ManyToOneToServerTranslator<TModelCreate, TModel> :
        CrudToServerTranslator<TModelCreate, TModel>,
        ICrudManyToOne<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrudManyToOne<TModelCreate, TModel, string> _service;

        /// <inheritdoc />
        public ManyToOneToServerTranslator(ICrudable<TModel, string> service, string idConceptName, System.Func<string> getServerNameMethod, ITranslatorService translatorService)
            : base(service, idConceptName, getServerNameMethod, translatorService)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(idConceptName, nameof(idConceptName));
            InternalContract.RequireNotNull(getServerNameMethod, nameof(getServerNameMethod));
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            _service = new ManyToOnePassThrough<TModelCreate, TModel, string>(service);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
        CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(parentId).ExecuteAsync(cancellationToken );
            parentId = translator.Translate(parentId);
            return await _service.ReadChildrenWithPagingAsync(parentId, offset, limit, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(parentId).ExecuteAsync(cancellationToken );
            parentId = translator.Translate(parentId);
            return await _service.ReadChildrenAsync(parentId, limit, cancellationToken );
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(string parentId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(parentId).ExecuteAsync(cancellationToken );
            parentId = translator.Translate(parentId);
            await _service.DeleteChildrenAsync(parentId, cancellationToken );
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