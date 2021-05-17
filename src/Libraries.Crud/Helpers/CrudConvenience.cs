using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.PassThrough;

namespace Nexus.Link.Libraries.Crud.Helpers
{
    public class CrudConvenience<TModelCreate, TModel, TId> : ICreateAndReturn<TModelCreate, TModel, TId>, IReadAll<TModel, TId>
        where TModel : TModelCreate
    {
        private readonly ICrud<TModelCreate, TModel, TId> _service;

        public CrudConvenience(ICrudable<TModel, TId> service)
        {
            _service = new CrudPassThrough<TModelCreate,TModel, TId>(service);
        }

        /// <inheritdoc />
        public async Task<TModel> CreateAndReturnAsync(TModelCreate item, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireNotNull(item, nameof(item));
            var id = await _service.CreateAsync(item, token);
            return await _service.ReadAsync(id, token);
        }

        /// <inheritdoc />
        public Task<IEnumerable<TModel>> ReadAllAsync(int limit = Int32.MaxValue, CancellationToken token = default(CancellationToken))
        {
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));
            return StorageHelper.ReadPagesAsync<TModel>((offset, ct) => _service.ReadAllWithPagingAsync(offset, null, ct), limit, token);
        }
    }
}