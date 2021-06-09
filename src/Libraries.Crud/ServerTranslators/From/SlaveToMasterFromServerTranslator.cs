using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ServerTranslators.From
{
    /// <inheritdoc cref="SlaveToMasterFromServerTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class SlaveToMasterFromServerTranslator<TModel> : 
        SlaveToMasterFromServerTranslator<TModel, TModel>,
        ICrudSlaveToMaster<TModel, string>
    {
        /// <inheritdoc />
        public SlaveToMasterFromServerTranslator(ICrudable<TModel, string> service,
            string masterIdConceptName, string slaveIdConceptName, System.Func<string> getServerNameMethod)
            : base(service, masterIdConceptName, slaveIdConceptName, getServerNameMethod)
        {
        }
    }

    /// <inheritdoc cref="ServerTranslatorBase" />
    [Obsolete("Use Libraries.Web ValueTranslatorHttpSender. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class SlaveToMasterFromServerTranslator<TModelCreate, TModel> : 
        ServerTranslatorBase,
        ICrudSlaveToMaster<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private readonly ICrudSlaveToMaster<TModelCreate, TModel, string> _service;
        private readonly string _masterIdConceptName;

        /// <inheritdoc />
        public SlaveToMasterFromServerTranslator(ICrudable<TModel, string> service, string masterIdConceptName, string slaveIdConceptName, System.Func<string> getServerNameMethod)
            : base(slaveIdConceptName, getServerNameMethod, new FakeTranslatorService())
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(masterIdConceptName, nameof(masterIdConceptName));
            InternalContract.RequireNotNullOrWhiteSpace(slaveIdConceptName, nameof(slaveIdConceptName));
            InternalContract.RequireNotNull(getServerNameMethod, nameof(getServerNameMethod));
            _service = new SlaveToMasterPassThrough<TModelCreate, TModel, string>(service);
            _masterIdConceptName = masterIdConceptName;
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(string masterId, TModelCreate item, CancellationToken token = default)
        {
            var result = await _service.CreateAsync(masterId, item, token);
            var translator = CreateTranslator();
            return translator.Decorate(IdConceptName, result);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(string masterId, TModelCreate item, CancellationToken token = default)
        {
            var result = await _service.CreateAndReturnAsync(masterId, item, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken token = default)
        {
            await _service.CreateWithSpecifiedIdAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken token = default)
        {
            var result = await _service.CreateWithSpecifiedIdAndReturnAsync(masterId, slaveId, item, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(string masterId, string slaveId, CancellationToken token = default)
        {
            var result = await _service.ReadAsync(masterId, slaveId, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public Task<TModel> ReadAsync(SlaveToMasterId<string> id, CancellationToken token = default)
        {
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
            CancellationToken token = default)
        {
            var result = await _service.ReadChildrenWithPagingAsync(parentId, offset, limit, token);
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            var result = await _service.ReadChildrenAsync(parentId, limit, token);
            var translator = CreateTranslator();
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public Task UpdateAsync(string masterId, string slaveId, TModel item, CancellationToken token = default)
        {
            return _service.UpdateAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(string masterId, string slaveId, TModel item,
            CancellationToken token = default)
        {
            var result = await _service.UpdateAndReturnAsync(masterId, slaveId, item, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.Decorate(result);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(string masterId, CancellationToken token = default)
        {
            return _service.DeleteChildrenAsync(masterId, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string masterId, string slaveId, CancellationToken token = default)
        {
            return _service.DeleteAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public async Task<SlaveLock<string>> ClaimLockAsync(string masterId, string slaveId, CancellationToken token = default)
        {
            var result = await _service.ClaimLockAsync(masterId, slaveId, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            result.MasterId = translator.Decorate(_masterIdConceptName, result.MasterId);
            result.SlaveId = translator.Decorate(IdConceptName, result.SlaveId);
            return result;
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(string masterId, string slaveId, string lockId,
            CancellationToken token = default)
        {
            return _service.ReleaseLockAsync(masterId, slaveId, lockId, token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<string>> ClaimDistributedLockAsync(string masterId, string slaveId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(string masterId, string slaveId, string lockId,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(string masterId, string slaveId, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TModel> ClaimTransactionLockAndReadAsync(string masterId, string slaveId,
            CancellationToken token = default)
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