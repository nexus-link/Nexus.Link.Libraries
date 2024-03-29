﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ServerTranslators.From
{
    /// <inheritdoc cref="ManyToOneFromServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class ManyToOneFromServerTranslator<TModel> :
        ManyToOneFromServerTranslator<TModel, TModel>,
        ICrudManyToOne<TModel, string>
    {

        /// <inheritdoc />
        public ManyToOneFromServerTranslator(ICrudable<TModel, string> service, string idConceptName,
            System.Func<string> getServerNameMethod)
            : base(service, idConceptName, getServerNameMethod)
        {
        }
    }

    /// <inheritdoc cref="CrudFromServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class ManyToOneFromServerTranslator<TModelCreate, TModel> :
        CrudFromServerTranslator<TModelCreate, TModel>,
        ICrudManyToOne<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrudManyToOne<TModelCreate, TModel, string> _service;

        /// <inheritdoc />
        public ManyToOneFromServerTranslator(ICrudable<TModel, string> service, string idConceptName, System.Func<string> getServerNameMethod)
            : base(service, idConceptName, getServerNameMethod)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(idConceptName, nameof(idConceptName));
            InternalContract.RequireNotNull(getServerNameMethod, nameof(getServerNameMethod));
            _service = new ManyToOnePassThrough<TModelCreate, TModel, string>(service);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
        CancellationToken cancellationToken  = default)
        {
            var result = await _service.ReadChildrenWithPagingAsync(parentId, offset, limit, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var result = await _service.ReadChildrenAsync(parentId, limit, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(string parentId, CancellationToken cancellationToken  = default)
        {
            return _service.DeleteChildrenAsync(parentId, cancellationToken );
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