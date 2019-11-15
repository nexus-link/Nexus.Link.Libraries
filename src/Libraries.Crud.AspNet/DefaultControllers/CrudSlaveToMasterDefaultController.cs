﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.AspNet.Controllers;
using Nexus.Link.Libraries.Crud.Interfaces;
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http;
#endif

namespace Nexus.Link.Libraries.Crud.AspNet.DefaultControllers
{
    /// <summary>
    /// ApiController with CRUD-support
    /// </summary>
    public class CrudSlaveToMasterDefaultController<TModel> :
        CrudSlaveToMasterDefaultController<TModel, TModel>,
        ICrudSlaveToMaster<TModel, string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CrudSlaveToMasterDefaultController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <inheritdoc cref="CrudSlaveToMasterController{TModelCreate, TModel}" />
    public class CrudSlaveToMasterDefaultController<TModelCreate, TModel> :
        CrudSlaveToMasterController<TModelCreate, TModel>,
        ICrudSlaveToMaster<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CrudSlaveToMasterDefaultController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("{masterId}/Children")]
        public override Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string masterId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return base.ReadChildrenWithPagingAsync(masterId, offset, limit, token);
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("{masterId}/Children")]
        public override Task<string> CreateAsync(string masterId, TModelCreate item, CancellationToken token = new CancellationToken())
        {
            return base.CreateAsync(masterId, item, token);
        }

        /// <inheritdoc />
        [HttpDelete]
        [Route("{masterId}/Children")]
        public override Task DeleteChildrenAsync(string masterId, CancellationToken token = new CancellationToken())
        {
            return base.DeleteChildrenAsync(masterId, token);
        }

        /// <inheritdoc cref="ICrudSlaveToMaster{TModel,TId}" />
        [HttpGet]
        [Route("{masterId}/Children/{slaveId}")]
        public override Task<TModel> ReadAsync(string masterId, string slaveId, CancellationToken token = new CancellationToken())
        {
            return base.ReadAsync(masterId, slaveId, token);
        }

        /// <inheritdoc cref="ICrudSlaveToMaster{TModel,TId}" />
        [HttpPut]
        [Route("{masterId}/Children/{slaveId}")]
        public override Task UpdateAsync(string masterId, string slaveId, TModel item, CancellationToken token = new CancellationToken())
        {
            return base.UpdateAsync(masterId, slaveId, item, token);
        }

        /// <inheritdoc cref="ICrudSlaveToMaster{TModel,TId}" />
        [HttpDelete]
        [Route("{masterId}/Children/{slaveId}")]
        public override Task DeleteAsync(string masterId, string slaveId, CancellationToken token = new CancellationToken())
        {
            return base.DeleteAsync(masterId, slaveId, token);
        }
    }
}