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
        public async Task<IEnumerable<TModel>> ReadAllAsync(int limit = Int32.MaxValue, CancellationToken token = default(CancellationToken))
        {
            var pageEnumerator = new PageEnvelopeEnumeratorAsync<TModel>((offset, cancellationToken) =>
                _service.ReadAllWithPagingAsync(offset, null, cancellationToken), token);
            var list = new List<TModel>();
            var count = 0;
            while (await pageEnumerator.MoveNextAsync())
            {
                list.Add(pageEnumerator.Current);
                count++;
                if (count >= limit) break;
            }

            return list;
        }
    }
}