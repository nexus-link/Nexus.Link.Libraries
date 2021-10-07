using System;
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
    /// <inheritdoc cref="CrudFromServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class CrudFromServerTranslator<TModel> : CrudFromServerTranslator<TModel, TModel>, ICrud<TModel, string>
    {
        /// <inheritdoc />
        public CrudFromServerTranslator(ICrudable<TModel, string> service, string idConceptName,
            System.Func<string> getServerNameMethod)
            : base(service, idConceptName, getServerNameMethod)
        {
        }
    }

    /// <inheritdoc cref="ServerTranslatorBase" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class CrudFromServerTranslator<TModelCreate, TModel> : ServerTranslatorBase, ICrud<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrud<TModelCreate, TModel, string> _service;

        /// <inheritdoc />
        public CrudFromServerTranslator(ICrudable<TModel, string> service, string idConceptName, System.Func<string> getServerNameMethod)
            : base(idConceptName, getServerNameMethod, new FakeTranslatorService())
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(idConceptName, nameof(idConceptName));
            InternalContract.RequireNotNull(getServerNameMethod, nameof(getServerNameMethod));
            _service = new CrudPassThrough<TModelCreate, TModel, string>(service);
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var id = await _service.CreateAsync(item, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(IdConceptName, id);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var decoratedResult = await _service.CreateAndReturnAsync(item, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(decoratedResult);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(string id, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            return _service.CreateWithSpecifiedIdAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string id, TModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            var decoratedResult = await _service.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(decoratedResult);
        }

        /// <inheritdoc />
        public virtual async Task<TModel> ReadAsync(string id, CancellationToken cancellationToken  = default)
        {
            var result = await _service.ReadAsync(id, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            var result = await _service.ReadAllWithPagingAsync(offset, limit, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TModel>> ReadAllAsync(int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var result = await _service.ReadAllAsync(limit, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(string id, TModel item, CancellationToken cancellationToken  = default)
        {
            await _service.UpdateAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(string id, TModel item, CancellationToken cancellationToken  = default)
        {
            var result = await _service.UpdateAndReturnAsync(id, item, cancellationToken );
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string id, CancellationToken cancellationToken  = default)
        {
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