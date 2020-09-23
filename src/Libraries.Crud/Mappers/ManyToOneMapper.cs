﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.Mappers
{
    /// <inheritdoc cref="ManyToOneMapper{TClientModelCreate,TClientModel,TClientId,TServerModel,TServerId}" />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete since 2020-09-23.")]
    public class ManyToOneMapper<TClientModel, TClientId, TServerModel, TServerId> :
        ManyToOneMapper<TClientModel, TClientModel, TClientId, TServerModel, TServerId>,
        ICrudManyToOne<TClientModel, TClientId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ManyToOneMapper(ICrudable<TServerModel, TServerId> service, IMappable<TClientModel, TServerModel> mapper)
            : base(service, mapper)
        {
        }
    }

    /// <inheritdoc cref="ICrudManyToOne{TManyModelCreate,TManyModel,TClientId}" />
    [Obsolete("We no longer recommend to use this mapping technique. Obsolete since 2020-09-23.")]
    public class ManyToOneMapper<TClientModelCreate, TClientModel, TClientId, TServerModel, TServerId> : CrudMapper<TClientModelCreate, TClientModel, TClientId, TServerModel, TServerId>, ICrudManyToOne<TClientModelCreate, TClientModel, TClientId> where TClientModel : TClientModelCreate
    {
        private readonly ICrudManyToOne<TServerModel, TServerId> _service;
        private readonly IMapper<TClientModelCreate, TClientModel, TServerModel> _mapper;
        /// <summary>
        /// Constructor
        /// </summary>
        public ManyToOneMapper(ICrudable<TServerModel, TServerId> service, IMappable<TClientModel, TServerModel> mapper)
            : base(service, mapper)
        {
            InternalContract.RequireNotNull(service, nameof(service));
            InternalContract.RequireNotNull(mapper, nameof(mapper));
            _service = new ManyToOnePassThrough<TServerModel, TServerId>(service);
            _mapper = new MapperPassThrough<TClientModelCreate, TClientModel, TServerModel>(mapper);
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TClientModel>> ReadChildrenWithPagingAsync(TClientId parentId, int offset, int? limit = null,
            CancellationToken token = default(CancellationToken))
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(parentId);
            var storagePage = await _service.ReadChildrenWithPagingAsync(serverId, offset, limit, token);
            FulcrumAssert.IsNotNull(storagePage?.Data);
            var data = storagePage?.Data.Select(_mapper.MapFromServer);
            return new PageEnvelope<TClientModel>(storagePage?.PageInfo, data);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TClientModel>> ReadChildrenAsync(TClientId parentId, int limit = Int32.MaxValue, CancellationToken token = default(CancellationToken))
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(parentId);
            var items = await  _service.ReadChildrenAsync(serverId, limit, token);
            FulcrumAssert.IsNotNull(items);
            return items?.Select(_mapper.MapFromServer);
        }

        /// <inheritdoc />
        public virtual Task DeleteChildrenAsync(TClientId parentId, CancellationToken token = default(CancellationToken))
        {
            var serverId = MapperHelper.MapToType<TServerId, TClientId>(parentId);
            return _service.DeleteChildrenAsync(serverId, token);
        }
    }
}
