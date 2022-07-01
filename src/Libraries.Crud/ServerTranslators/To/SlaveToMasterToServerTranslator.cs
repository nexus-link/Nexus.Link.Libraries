using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ServerTranslators.To
{
    /// <inheritdoc cref="SlaveToMasterToServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class SlaveToMasterToServerTranslator<TModel> :
        SlaveToMasterToServerTranslator<TModel, TModel>,
        ICrudSlaveToMaster<TModel, string>
    {
        /// <inheritdoc />
        public SlaveToMasterToServerTranslator(ICrudable<TModel, string> service,
            System.Func<string> getServerNameMethod, ITranslatorService translatorService)
            : base(service, getServerNameMethod, translatorService)
        {
        }
    }

    /// <inheritdoc cref="ServerTranslatorBase" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class SlaveToMasterToServerTranslator<TModelCreate, TModel> : 
        ServerTranslatorBase, 
        ICrudSlaveToMaster<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrudSlaveToMaster<TModelCreate, TModel, string> _service;

        /// <inheritdoc />
        public SlaveToMasterToServerTranslator(ICrudable<TModel, string> service, System.Func<string> getServerNameMethod, ITranslatorService translatorService)
            : base(null, getServerNameMethod, translatorService)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNull(getServerNameMethod, nameof(getServerNameMethod));
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            _service = new SlaveToMasterPassThrough<TModelCreate, TModel, string>(service);
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(string masterId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(item).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            item = translator.Translate(item);
            return await _service.CreateAsync(masterId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(string masterId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(item).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            item = translator.Translate(item);
            return await _service.CreateAndReturnAsync(masterId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string masterId, string slaveId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).Add(item).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            item = translator.Translate(item);
            await _service.CreateWithSpecifiedIdAsync(masterId, slaveId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string masterId, string slaveId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).Add(item).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            item = translator.Translate(item);
            return await _service.CreateWithSpecifiedIdAndReturnAsync(masterId, slaveId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            return await _service.ReadAsync(masterId, slaveId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<TModel> ReadAsync(SlaveToMasterId<string> id, CancellationToken cancellationToken  = default)
        {
            return ReadAsync(id.MasterId, id.SlaveId, cancellationToken );
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
        public async Task UpdateAsync(string masterId, string slaveId, TModel item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).Add(item).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            item = translator.Translate(item);
            await _service.UpdateAsync(masterId, slaveId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(string masterId, string slaveId, TModel item,
            CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).Add(item).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            item = translator.Translate(item);
            return await _service.UpdateAndReturnAsync(masterId, slaveId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task DeleteChildrenAsync(string masterId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            await _service.DeleteChildrenAsync(masterId, cancellationToken );
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            await _service.DeleteAsync(masterId, slaveId, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<SlaveLock<string>> ClaimLockAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            return await _service.ClaimLockAsync(masterId, slaveId, cancellationToken );
        }

        /// <inheritdoc />
        public async Task ReleaseLockAsync(string masterId, string slaveId, string lockId,
            CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            await translator.Add(masterId).Add(slaveId).ExecuteAsync(cancellationToken );
            masterId = translator.Translate(masterId);
            slaveId = translator.Translate(slaveId);
            await _service.ReleaseLockAsync(masterId, slaveId, lockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<SlaveLock<string>> ClaimDistributedLockAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(string masterId, string slaveId, string lockId,
            CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(string masterId, string slaveId,
            CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
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