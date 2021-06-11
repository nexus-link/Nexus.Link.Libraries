using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Crud.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.Mappers
{
    /// <inheritdoc cref="SlaveToMasterMapper{TClientModelCreate,TClientModel,TClientId,TServerModel,TServerId}" />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete since 2020-09-23.")]
    public class SlaveToMasterMapper<TClientModel, TClientId, TServerModel, TServerId> :
        SlaveToMasterMapper<TClientModel, TClientModel, TClientId, TServerModel, TServerId>,
        ICrudSlaveToMaster<TClientModel, TClientId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SlaveToMasterMapper(ICrudable<TServerModel, TServerId> service, IMappable<TClientModel, TServerModel> mapper)
            : base(service, mapper)
        {
        }
    }

    /// <inheritdoc cref="ICrudSlaveToMaster{TModelCreate,TModel,TId}" />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete since 2020-09-23.")]
    public class SlaveToMasterMapper<TClientModelCreate, TClientModel, TClientId, TServerModel, TServerId> : 
        ICrudSlaveToMaster<TClientModelCreate, TClientModel, TClientId> 
        where TClientModel : TClientModelCreate
    {
        private readonly ICrudSlaveToMaster<TServerModel, TServerId> _service;
        private readonly IMapper<TClientModelCreate, TClientModel, TServerModel> _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public SlaveToMasterMapper(ICrudable<TServerModel, TServerId> service, IMappable<TClientModel, TServerModel> mapper)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNull(mapper, nameof(mapper));
            _service = new SlaveToMasterPassThrough<TServerModel, TServerId>(service);
            _mapper = new MapperPassThrough<TClientModelCreate, TClientModel, TServerModel>(mapper);
        }

        /// <inheritdoc />
        public virtual async Task<TClientId> CreateAsync(TClientId masterId, TClientModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var record = _mapper.MapToServer(item);
            var serverId = await _service.CreateAsync(serverMasterId, record, token);
            FulcrumAssert.IsNotDefaultValue(serverId);
            return MapperHelper.MapToType<TClientId, TServerId>(serverId);
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> CreateAndReturnAsync(TClientId masterId, TClientModelCreate item, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var record = _mapper.MapToServer(item);
            record = await _service.CreateAndReturnAsync(serverMasterId, record, token);
            FulcrumAssert.IsNotDefaultValue(record);
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public virtual async Task CreateWithSpecifiedIdAsync(TClientId masterId, TClientId slaveId, TClientModelCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            var record = _mapper.MapToServer(item);
            await _service.CreateWithSpecifiedIdAsync(serverMasterId, serverSlaveId, record, token);
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> CreateWithSpecifiedIdAndReturnAsync(TClientId masterId, TClientId slaveId, TClientModelCreate item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            var record = _mapper.MapToServer(item);
            record = await _service.CreateWithSpecifiedIdAndReturnAsync(serverMasterId, serverSlaveId, record, token);
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> ReadAsync(TClientId masterId, TClientId slaveId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            var record = await _service.ReadAsync(serverMasterId, serverSlaveId, token);
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public Task<TClientModel> ReadAsync(SlaveToMasterId<TClientId> id, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(id, nameof(id));
            InternalContract.RequireValidated(id, nameof(id));
            return ReadAsync(id.MasterId, id.SlaveId, token);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TClientModel>> ReadChildrenWithPagingAsync(TClientId parentId, int offset, int? limit = null,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(parentId);
            var storagePage = await _service.ReadChildrenWithPagingAsync(serverId, offset, limit, token);
            FulcrumAssert.IsNotNull(storagePage?.Data);
            var data = storagePage?.Data.Select(_mapper.MapFromServer);
            return new PageEnvelope<TClientModel>(storagePage?.PageInfo, data);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TClientModel>> ReadChildrenAsync(TClientId parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(parentId);
            var items = await _service.ReadChildrenAsync(serverId, limit, token);
            FulcrumAssert.IsNotNull(items);
            return items?.Select(_mapper.MapFromServer);
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TClientId masterId, TClientId slaveId, TClientModel item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            var record = _mapper.MapToServer(item);
            return _service.UpdateAsync(serverMasterId, serverSlaveId, record, token);
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> UpdateAndReturnAsync(TClientId masterId, TClientId slaveId, TClientModel item,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            InternalContract.RequireNotNull(item, nameof(item));
            InternalContract.RequireValidated(item, nameof(item));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            var record = _mapper.MapToServer(item);
            record = await _service.UpdateAndReturnAsync(serverMasterId, serverSlaveId, record, token);
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TClientId parentId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(parentId, nameof(parentId));
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(parentId);
            return _service.DeleteChildrenAsync(serverId, token);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TClientId masterId, TClientId slaveId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            return _service.DeleteAsync(serverMasterId, serverSlaveId, token);
        }

        /// <inheritdoc />
        public virtual async Task<SlaveLock<TClientId>> ClaimLockAsync(TClientId masterId, TClientId slaveId, CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            var serverLock = await _service.ClaimLockAsync(serverMasterId, serverSlaveId, token);
            var clientLock = new SlaveLock<TClientId>
            {
                Id = MapperHelper.MapToType<TClientId, TServerId>(serverLock.Id),
                MasterId = MapperHelper.MapToType<TClientId, TServerId>(serverLock.MasterId),
                SlaveId = MapperHelper.MapToType<TClientId, TServerId>(serverLock.SlaveId)
            };
            return clientLock;
        }

        /// <inheritdoc />
        public virtual Task ReleaseLockAsync(TClientId masterId, TClientId slaveId, TClientId lockId,
            CancellationToken token = default)
        {
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(slaveId, nameof(slaveId));
            var serverMasterId = MapperHelper.MapToType<TServerId, TClientId>(masterId);
            var serverSlaveId = MapperHelper.MapToType<TServerId, TClientId>(slaveId);
            var serverLockId = MapperHelper.MapToType<TServerId, TClientId>(lockId);
            return _service.ReleaseLockAsync(serverMasterId, serverSlaveId, serverLockId, token);
        }

        /// <inheritdoc />
        public Task<SlaveLock<TClientId>> ClaimDistributedLockAsync(TClientId masterId, TClientId slaveId,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TClientId masterId, TClientId slaveId, TClientId lockId,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TClientId masterId, TClientId slaveId,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TClientModel> ClaimTransactionLockAndReadAsync(TClientId masterId, TClientId slaveId,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TClientModel>> SearchChildrenAsync(TClientId parentId, SearchDetails<TClientModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TClientModel> FindUniqueChildAsync(TClientId parentId, SearchDetails<TClientModel> details,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
