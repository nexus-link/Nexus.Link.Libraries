using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.ClientTranslators
{
    /// <inheritdoc cref="SlaveToMasterClientTranslator{TModelCreate, TModel}" />
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class SlaveToMasterClientTranslator<TModel> : 
        SlaveToMasterClientTranslator<TModel, TModel>,
        ICrudSlaveToMaster<TModel, string>
    {
        /// <inheritdoc />
        public SlaveToMasterClientTranslator(ICrudable<TModel, string> service,
            string masterIdConceptName, string slaveIdConceptName, System.Func<string> getClientNameMethod,
            ITranslatorService translatorService)
            : base(service, masterIdConceptName, slaveIdConceptName, getClientNameMethod, translatorService)
        {
        }
    }

    /// <inheritdoc cref="ClientTranslatorBase" />
    [Obsolete("Use Libraries.Web.AspNet ValueTranslatorFilter. Obsolete warning since 2019-11-21, error since 2021-06-09.", true)]
    public class SlaveToMasterClientTranslator<TModelCreate, TModel> :
        ClientTranslatorBase,
        ICrudSlaveToMaster<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        private ICrudSlaveToMaster<TModelCreate, TModel, string> Service { get; }
        private readonly string _masterIdConceptName;

        /// <inheritdoc />
        public SlaveToMasterClientTranslator(ICrudable<TModel, string> service, string masterIdConceptName, string slaveIdConceptName, System.Func<string> getClientNameMethod, ITranslatorService translatorService)
            : base(slaveIdConceptName, getClientNameMethod, translatorService)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNullOrWhiteSpace(masterIdConceptName, nameof(masterIdConceptName));
            InternalContract.RequireNotNullOrWhiteSpace(slaveIdConceptName, nameof(slaveIdConceptName));
            InternalContract.RequireNotNull(getClientNameMethod, nameof(getClientNameMethod));
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            _masterIdConceptName = masterIdConceptName;
            Service = new SlaveToMasterPassThrough<TModelCreate, TModel, string>(service);
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(string masterId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            item = translator.Decorate(item);
            var decoratedResult = await Service.CreateAsync(masterId, item, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(string masterId, TModelCreate item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            item = translator.Decorate(item);
            var decoratedResult = await Service.CreateAndReturnAsync(masterId, item, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public Task CreateWithSpecifiedIdAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            item = translator.Decorate(item);
            return Service.CreateWithSpecifiedIdAsync(masterId, slaveId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> CreateWithSpecifiedIdAndReturnAsync(string masterId, string slaveId, TModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            item = translator.Decorate(item);
            var decoratedResult = await Service.CreateWithSpecifiedIdAndReturnAsync(masterId, slaveId, item, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public async Task<TModel> ReadAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            var decoratedResult = await Service.ReadAsync(masterId, slaveId, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
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
            parentId = translator.Decorate(_masterIdConceptName, parentId);
            var decoratedResult = await Service.ReadChildrenWithPagingAsync(parentId, offset, limit, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TModel>> ReadChildrenAsync(string parentId, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            parentId = translator.Decorate(_masterIdConceptName, parentId);
            var decoratedResult = await Service.ReadChildrenAsync(parentId, limit, cancellationToken );
            var array = decoratedResult as TModel[] ?? decoratedResult.ToArray();
            await translator.Add(array).ExecuteAsync(cancellationToken );
            return translator.Translate(array);
        }

        /// <inheritdoc />
        public Task UpdateAsync(string masterId, string slaveId, TModel item, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            item = translator.Decorate(item);
            return Service.UpdateAsync(masterId, slaveId, item, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<TModel> UpdateAndReturnAsync(string masterId, string slaveId, TModel item,
            CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            item = translator.Decorate(item);
            var decoratedResult = await Service.UpdateAndReturnAsync(masterId, slaveId, item, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public Task DeleteAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            return Service.DeleteAsync(masterId, slaveId, cancellationToken );
        }

        /// <inheritdoc />
        public Task DeleteChildrenAsync(string masterId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            return Service.DeleteChildrenAsync(masterId, cancellationToken );
        }

        /// <inheritdoc />
        public async Task<SlaveLock<string>> ClaimLockAsync(string masterId, string slaveId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            var decoratedResult = await Service.ClaimLockAsync(masterId, slaveId, cancellationToken );
            await translator.Add(decoratedResult).ExecuteAsync(cancellationToken );
            return translator.Translate(decoratedResult);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(string masterId, string slaveId, string lockId, CancellationToken cancellationToken  = default)
        {
            var translator = CreateTranslator();
            masterId = translator.Decorate(_masterIdConceptName, masterId);
            slaveId = translator.Decorate(IdConceptName, slaveId);
            return Service.ReleaseLockAsync(masterId, slaveId, lockId, cancellationToken );
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