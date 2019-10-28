using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.AspNet.Controllers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http;
#endif

namespace Nexus.Link.Libraries.Crud.AspNet.DefaultControllers
{
    /// <inheritdoc cref="CrudDefaultController{TModelCreate, TModel}" />
    public class CrudDefaultController<TModel> : CrudDefaultController<TModel, TModel>, ICrud<TModel, string>
    {
        /// <inheritdoc />
        public CrudDefaultController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }
    }

    /// <inheritdoc cref="CrudController{TModel}" />
    public class CrudDefaultController<TModelCreate, TModel> :
        CrudController<TModelCreate, TModel>,
        ICrud<TModelCreate, TModel, string>
        where TModel : TModelCreate
    {
        /// <inheritdoc />
        public CrudDefaultController(ICrudable<TModel, string> logic)
            : base(logic)
        {
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("{id}")]
        public override Task<TModel> ReadAsync(string id, CancellationToken token = default(CancellationToken))
        {
            return base.ReadAsync(id, token);
        }

        /// <inheritdoc />
        [HttpGet]
        [Route("")]
        public override Task<PageEnvelope<TModel>> ReadAllWithPagingAsync(int offset, int? limit = null, CancellationToken token = default(CancellationToken))
        {

            return base.ReadAllWithPagingAsync(offset, limit, token);
        }

        /// <inheritdoc />
        [HttpPost]
        [Route("")]
        public override Task<string> CreateAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            return base.CreateAsync(item, token);
        }

        /// <inheritdoc />
        [HttpPut]
        [Route("{id}")]
        public override Task UpdateAsync(string id, TModel item, CancellationToken token = default(CancellationToken))
        {
            return base.UpdateAsync(id, item, token);
        }

        /// <inheritdoc />
        [HttpDelete]
        [Route("{id}")]
        public override Task DeleteAsync(string id, CancellationToken token = default(CancellationToken))
        {
            return base.DeleteAsync(id, token);
        }

        /// <inheritdoc />
        [HttpDelete]
        [Route("")]
        public override Task DeleteAllAsync(CancellationToken token = default(CancellationToken))
        {
            return base.DeleteAllAsync(token);
        }
    }
}