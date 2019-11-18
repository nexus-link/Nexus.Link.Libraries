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
        public async Task<string> CreateAsync(string masterId, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            var result = await _service.CreateAsync(masterId, item, token);
            var translator = CreateTranslator();
            return translator.Decorate(IdConceptName, result);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(string masterId, TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            var result = await _service.CreateAndReturnAsync(masterId, item, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.DecorateItem(result);
        }

        /// <inheritdoc />
        public async Task CreateWithSpecifiedIdAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken token = default(CancellationToken))
        {
            await _service.CreateWithSpecifiedIdAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken token = default(CancellationToken))
        {
            var result = await _service.CreateWithSpecifiedIdAndReturnAsync(masterId, slaveId, item, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.DecorateItem(result);
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(string masterId, string slaveId, CancellationToken token = default(CancellationToken))
        {
            var result = await _service.ReadAsync(masterId, slaveId, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.DecorateItem(result);
        }

        /// <inheritdoc />
        public Task<TModel> ReadAsync(SlaveToMasterId<string> id, CancellationToken token = default(CancellationToken))
        {
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public async Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null,
            CancellationToken token = default(CancellationToken))
        {
            var result = await _service.ReadChildrenWithPagingAsync(parentId, offset, limit, token);
            var translator = CreateTranslator();
            return translator.DecorateItem(result);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken token = default(CancellationToken))
        {
            var result = await _service.ReadChildrenAsync(parentId, limit, token);
            var translator = CreateTranslator();
            return translator.DecorateItems(result);
        }

        /// <inheritdoc />
        public Task UpdateAsync(string masterId, string slaveId, TModel item, CancellationToken token = default(CancellationToken))
        {
            return _service.UpdateAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(string masterId, string slaveId, TModel item,
            CancellationToken token = default(CancellationToken))
        {
            var result = await _service.UpdateAndReturnAsync(masterId, slaveId, item, token);
            var translator = CreateTranslator();
            FulcrumAssert.IsNotNull(result);
            return translator.DecorateItem(result);
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(string masterId, CancellationToken token = default(CancellationToken))
        {
            return _service.DeleteChildrenAsync(masterId, token);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string masterId, string slaveId, CancellationToken token = default(CancellationToken))
        {
            return _service.DeleteAsync(masterId, slaveId, token);
        }

        /// <inheritdoc />
        public async Task<SlaveLock<string>> ClaimLockAsync(string masterId, string slaveId, CancellationToken token = default(CancellationToken))
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
            CancellationToken token = default(CancellationToken))
        {
            return _service.ReleaseLockAsync(masterId, slaveId, lockId, token);
        }
    }
}