using System;
using System.Threading;
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
    [Obsolete("Use Nexus.Link.Libraries.Crud.AspNet.Controllers classes. Obsolete since 2020-09-23.")]
    public class CrudManyToOneDefaultController<TModel> :
        CrudManyToOneDefaultController<TModel, TModel>,
        ICrudManyToOne<TModel, string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CrudManyToOneDefaultController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <summary>
    /// ApiController with CRUD-support
    /// </summary>
    [Obsolete("Use Nexus.Link.Libraries.Crud.AspNet.Controllers classes. Obsolete since 2020-09-23.")]
    public class CrudManyToOneDefaultController<TModelCreate, TModel> :
        CrudManyToOneController<TModelCreate, TModel>,
        ICrudManyToOne<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CrudManyToOneDefaultController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("")]
        public override Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(string parentId, int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {
            return base.ReadChildrenWithPagingAsync(parentId, offset, limit, token);
        }

        /// <inheritdoc />
        [HttpDelete]
        [Route("")]
        public override Task DeleteChildrenAsync(string parentId, CancellationToken token = new CancellationToken())
        {
            return base.DeleteChildrenAsync(parentId, token);
        }
    }
}