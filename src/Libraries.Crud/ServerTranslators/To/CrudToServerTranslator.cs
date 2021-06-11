using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ServerTranslators.To
{
    /// <inheritdoc cref="CrudToServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class CrudToServerTranslator<TModel> : CrudToServerTranslator<TModel, TModel>, ICrud<TModel, string>
    {
        /// <inheritdoc />
        public CrudToServerTranslator(ICrudable<TModel, string> service, string idConceptName,
            System.Func<string> getServerNameMethod, ITranslatorService translatorService)
            : base(service, idConceptName, getServerNameMethod, translatorService)
        {
        }
    }

    /// <inheritdoc cref="ServerTranslatorBase" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class CrudToServerTranslator<TModelCreate, TModel> : ServerTranslatorBase, ICrud<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrud<TModelCreate, TModel, string> _service;

        /// <inheritdoc />
        public CrudToServerTranslator(ICrudable<TModel, string> service, string idConceptName, System.Func<string> getServerNameMethod, ITranslatorService translatorService)
            : base(idConceptName, getServerNameMethod, translatorService)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(idConceptName, nameof(idConceptName));
            InternalContract.RequireNotNull(getServerNameMethod, nameof(getServerNameMethod));
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            _service = new CrudPassThrough<TModelCreate, TModel, string>(service);
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(TModelCreate item, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(item).ExecuteAsync(token);
            item = translator.Translate(item);
            return await _service.CreateAsync(item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(item).ExecuteAsync(token);
            item = translator.Translate(item);
            return await _service.CreateAndReturnAsync(item, token);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string id, TModelCreate item, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).Add(item).ExecuteAsync(token);
            id = translator.Translate(id);
            item = translator.Translate(item);
            await _service.CreateWithSpecifiedIdAsync(id, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string id, TModelCreate item,
            CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).Add(item).ExecuteAsync(token);
            id = translator.Translate(id);
            item = translator.Translate(item);
            return await _service.CreateWithSpecifiedIdAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> ReadAsync(string id, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).ExecuteAsync(token);
            id = translator.Translate(id);
            return await _service.ReadAsync(id, token);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default)
        {
            return await _service.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken token = default)
        {
            return await _service.ReadAllAsync(limit, token);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, TModel item, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).Add(item).ExecuteAsync(token);
            id = translator.Translate(id);
            item = translator.Translate(item);
            await _service.UpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(string id, TModel item, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).Add(item).ExecuteAsync(token);
            id = translator.Translate(id);
            item = translator.Translate(item);
            return await _service.UpdateAndReturnAsync(id, item, token);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string id, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).ExecuteAsync(token);
            id = translator.Translate(id);
            await _service.DeleteAsync(id, token);
        }

        /// <inheritdoc />
        public Task DeleteAllAsync(CancellationToken token = default)
        {
            return _service.DeleteAllAsync(token);
        }

        /// <inheritdoc />
        public async Task<Lock<string>> ClaimLockAsync(string id, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).ExecuteAsync(token);
            id = translator.Translate(id);
            return await _service.ClaimLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(string id, string lockId, CancellationToken token = default)
        {
            return _service.ReleaseLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public async Task<Lock<string>> ClaimDistributedLockAsync(string id, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).ExecuteAsync(token);
            id = translator.Translate(id);
            return await _service.ClaimDistributedLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(string id, string lockId, CancellationToken token = default)
        {
            return _service.ReleaseDistributedLockAsync(id, lockId, token);
        }

        /// <inheritdoc />
        public async Task ClaimTransactionLockAsync(string id, CancellationToken token = default)
        {
            var translator = CreateTranslator();
            await translator.Add(id).ExecuteAsync(token);
            id = translator.Translate(id);
            await _service.ClaimTransactionLockAsync(id, token);
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(string id, CancellationToken token = default)
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