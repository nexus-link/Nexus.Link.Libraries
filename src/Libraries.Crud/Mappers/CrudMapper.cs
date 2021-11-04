using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.Mappers
{
    /// <inheritdoc cref="CrudMapper{TClientModelCreate,TClientModel,TClientId,TServerModel,TServerId}" />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public class
        CrudMapper<TClientModel, TClientId, TServerModel, TServerId> : CrudMapper<TClientModel, TClientModel, TClientId, TServerModel, TServerId>, ICrud<TClientModel, TClientId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CrudMapper(ICrudable<TServerModel, TServerId> service, IMappable<TClientModel, TServerModel> mapper)
        :base(service, mapper)
        {
        }
    }

    /// <inheritdoc cref="ICrud{TModel,TId}" />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete warning since 2020-09-23, error since 2021-06-09.", true)]
    public class CrudMapper<TClientModelCreate, TClientModel, TClientId, TServerModel, TServerId> : ICrud<TClientModelCreate, TClientModel, TClientId>
        where TClientModel : TClientModelCreate
    {
        private readonly ICrud<TServerModel, TServerId> _service;
        private readonly IMapper<TClientModelCreate, TClientModel, TServerModel> _mapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public CrudMapper(ICrudable<TServerModel, TServerId> service, IMappable<TClientModel, TServerModel> mapper)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNull(mapper, nameof(mapper));
            _service = new CrudPassThrough<TServerModel, TServerId>(service);
            _mapper = new MapperPassThrough<TClientModelCreate, TClientModel, TServerModel>(mapper);
        }

        /// <inheritdoc />
        public virtual async Task<TClientId> CreateAsync(TClientModelCreate item, CancellationToken cancellationToken  = default)
        {
            var record = _mapper.MapToServer(item);
            var serverId = await _service.CreateAsync(record, cancellationToken );
            FulcrumAssert.IsNotDefaultValue(serverId);
            return MapperHelper.MapToType<TClientId, TServerId>(serverId);
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> CreateAndReturnAsync(TClientModelCreate item, CancellationToken cancellationToken  = default)
        {
            var record = _mapper.MapToServer(item);
            record = await _service.CreateAndReturnAsync(record, cancellationToken );
            FulcrumAssert.IsNotDefaultValue(record);
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public virtual Task CreateWithSpecifiedIdAsync(TClientId id, TClientModelCreate item, CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            var record = _mapper.MapToServer(item);
            return _service.CreateWithSpecifiedIdAsync(serverId, record, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> CreateWithSpecifiedIdAndReturnAsync(TClientId id, TClientModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            var record = _mapper.MapToServer(item);
            record = await _service.CreateWithSpecifiedIdAndReturnAsync(serverId, record, cancellationToken );
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> ReadAsync(TClientId id, CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            var record = await _service.ReadAsync(serverId, cancellationToken );
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TClientModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            var storagePage = await _service.ReadAllWithPagingAsync(offset, limit, cancellationToken );
            FulcrumAssert.IsNotNull(storagePage?.Data);
            var data = storagePage?.Data.Select(_mapper.MapFromServer);
            return new PageEnvelope<TClientModel>(storagePage?.PageInfo, data);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TClientModel>> ReadAllAsync(int limit = 2147483647, CancellationToken cancellationToken  = default)
        {
            var items = await _service.ReadAllAsync(limit, cancellationToken );
            FulcrumAssert.IsNotNull(items);
            return items?.Select(_mapper.MapFromServer);
        }

        /// <inheritdoc />
        public virtual Task UpdateAsync(TClientId id, TClientModel item, CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            var record = _mapper.MapToServer(item);
            return _service.UpdateAsync(serverId, record, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<TClientModel> UpdateAndReturnAsync(TClientId id, TClientModel item, CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            var record = _mapper.MapToServer(item);
            record = await _service.UpdateAndReturnAsync(serverId, record, cancellationToken );
            return _mapper.MapFromServer(record);
        }

        /// <inheritdoc />
        public virtual Task DeleteAsync(TClientId id, CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            return _service.DeleteAsync(serverId, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task DeleteAllAsync(CancellationToken cancellationToken  = default)
        {
            return _service.DeleteAllAsync(cancellationToken );
        }

        /// <inheritdoc />
        public async Task<Lock<TClientId>> ClaimLockAsync(TClientId id, CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            var @lock = await _service.ClaimLockAsync(serverId, cancellationToken );
            return MapperHelper.MapToType<TClientId, TServerId>(@lock);
        }

        /// <inheritdoc />
        public Task ReleaseLockAsync(TClientId id, TClientId lockId, CancellationToken cancellationToken  = default)
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(id);
            var serverLockId = MapperHelper.MapToType<TServerId, TClientId>(lockId);
            return _service.ReleaseLockAsync(serverId, serverLockId, cancellationToken );
        }

        /// <inheritdoc />
        public Task<Lock<TClientId>> ClaimDistributedLockAsync(TClientId id, TimeSpan? lockTimeSpan = null,
            TClientId currentLockId = default,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ReleaseDistributedLockAsync(TClientId id, TClientId lockId, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ClaimTransactionLockAsync(TClientId id, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TClientModel> ClaimTransactionLockAndReadAsync(TClientId id, CancellationToken cancellationToken  = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TClientModel>> SearchAsync(SearchDetails<TClientModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<TClientModel> FindUniqueAsync(SearchDetails<TClientModel> details, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}