using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ClientTranslators
{
    /// <inheritdoc cref="CrudClientTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class CrudClientTranslator<TModel> : 
        CrudClientTranslator<TModel, TModel>, 
        ICrud<TModel, string>
    {
        /// <inheritdoc />
        public CrudClientTranslator(ICrudable<TModel, string> service, string idConceptName,
            System.Func<string> getClientNameMethod, ITranslatorService translatorService)
            : base(service, idConceptName, getClientNameMethod, translatorService)
        {
        }
    }

    /// <inheritdoc cref="ClientTranslatorBase" />
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class CrudClientTranslator<TModelCreate, TModel> : 
        ClientTranslatorBase, 
        ICrud<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrud<TModelCreate, TModel, string> _service;

        /// <inheritdoc />
        public CrudClientTranslator(ICrudable<TModel, string> service, string idConceptName, System.Func<string> getClientNameMethod, ITranslatorService translatorService)
            : base(idConceptName, getClientNameMethod, translatorService)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(idConceptName, nameof(idConceptName));
            InternalContract.RequireNotNull(getClientNameMethod, nameof(getClientNameMethod));
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            _service = new CrudPassThrough<TModelCreate, TModel, string>(service);
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            item = translator.Decorate(item);
            var decoratedId = await _service.CreateAsync(item, cancellationToken );
            await translator.Add(decoratedId).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedId);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            item = translator.Decorate(item);
            var decoratedResult = await _service.CreateAndReturnAsync(item, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string id, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            id = translator.Decorate(IdConceptName, id);
            item = translator.Decorate(item);
            await _service.CreateWithSpecifiedIdAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string id, TModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            id = translator.Decorate(IdConceptName, id);
            item = translator.Decorate(item);
            var decoratedResult = await _service.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> ReadAsync(string id, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            id = translator.Decorate(IdConceptName, id);
            var decoratedResult = await _service.ReadAsync(id, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            var decoratedResult = await _service.ReadAllWithPagingAsync(offset, limit, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            var decoratedResult = await _service.ReadAllAsync(limit, cancellationToken );
            var decoratedArray = decoratedResult as TModel[] ?? decoratedResult.ToArray();
            await translator.Add(decoratedArray).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedArray);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, TModel item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            id = translator.Decorate(IdConceptName, id);
            item = translator.Decorate(item);
            await _service.UpdateAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(string id, TModel item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            id = translator.Decorate(IdConceptName, id);
            item = translator.Decorate(item);
            var decoratedResult = await _service.UpdateAndReturnAsync(id, item, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string id, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            id = translator.Decorate(IdConceptName, id);
            return _service.DeleteAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task DeleteAllAsync(CancellationToken cancellationToken  = default)
        {
            return _service.DeleteAllAsync(cancellationToken );
        }

        /// <inheritdoc />
        public Task<Lock<string>> ClaimLockAsync(string id, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            id = translator.Decorate(IdConceptName, id);
            return _service.ClaimLockAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(string id, string lockId, CancellationToken cancellationToken  = default)
        {
            return _service.ReleaseLockAsync(id, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<Lock<string>> ClaimDistributedLockAsync(string id, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(string id, string lockId, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(string id, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(string id, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchAsync(SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueAsync(SearchDetails<TModel> details, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}