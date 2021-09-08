using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Model;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{

    /// <summary>
    /// A delegate method for getting the parent id from a stored item.
    /// </summary>
    /// <param name="item">The item to get the parent for.</param>
    public delegate object GetParentIdDelegate<in TModel>(TModel item);

    /// <summary>
    /// General class for storing a many to one item in memory.
    /// </summary>
    /// <typeparam name="TModel">The model for the children that each points out a parent.</typeparam>
    /// <typeparam name="TId">The type for the id field of the models.</typeparam>
    public class ManyToOneMemory<TModel, TId> : 
        ManyToOneMemory<TModel, TModel, TId>, 
        ICrud<TModel, TId>, 
        ICrudManyToOne<TModel, TId>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getParentIdDelegate">See <see cref="GetParentIdDelegate{TModel}"/>.</param>
        public ManyToOneMemory(GetParentIdDelegate<TModel> getParentIdDelegate)
        :base(getParentIdDelegate)
        {
        }
    }

    /// <summary>
    /// General class for storing a many to one item in memory.
    /// </summary>
    /// <typeparam name="TModel">The model for the children that each points out a parent.</typeparam>
    /// <typeparam name="TId">The type for the id field of the models.</typeparam>
    /// <typeparam name="TModelCreate"></typeparam>
    public class ManyToOneMemory<TModelCreate, TModel, TId> : 
        CrudMemory<TModelCreate, TModel, TId>, 
        ICrudManyToOne<TModelCreate, TModel, TId> 
        where TModel : TModelCreate
    {
        private readonly GetParentIdDelegate<TModel> _getParentIdDelegate;
        private readonly ManyToOneConvenience<TModelCreate, TModel, TId> _convenience;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getParentIdDelegate">See <see cref="GetParentIdDelegate{TModel}"/>.</param>
        public ManyToOneMemory(GetParentIdDelegate<TModel> getParentIdDelegate)
        {
            InternalContract.RequireNotNull(getParentIdDelegate, nameof(getParentIdDelegate));
            _getParentIdDelegate = getParentIdDelegate;
            _convenience = new ManyToOneConvenience<TModelCreate, TModel, TId>(this);
        }

        /// <inheritdoc />
        public virtual Task<PageEnvelope<TModel>> ReadChildrenWithPagingAsync(TId parentId, int offset, int? limit = null, CancellationToken token = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(parentId, nameof(parentId));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            lock (MemoryItems)
            {
                var list = MemoryItems.Values
                    .Where(i => parentId.Equals(_getParentIdDelegate(i)))
                    .Skip(offset)
                    .Take(limit.Value);
                var page = new PageEnvelope<TModel>(offset, limit.Value, MemoryItems.Count, list);
                return Task.FromResult(page);
            }
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TModel>> ReadChildrenAsync(TId parentId, int limit = int.MaxValue, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(parentId, nameof(parentId));
            InternalContract.RequireGreaterThan(0, limit, nameof(limit));


            var result = new List<TModel>();
            var offset = 0;
            while (true)
            {
                var page = await ReadChildrenWithPagingAsync(parentId, offset, null, token);
                if (page.PageInfo.Returned == 0) break;
                result.AddRange(page.Data);
                offset += page.PageInfo.Returned;
            }

            return result;
        }

        /// <inheritdoc />
        public virtual async Task DeleteChildrenAsync(TId masterId, CancellationToken token = default)
        {
            InternalContract.RequireNotNull(masterId, nameof(masterId));
            InternalContract.RequireNotDefaultValue(masterId, nameof(masterId));
            var errorMessage = $"{typeof(TModel).Name} must implement the interface {nameof(IUniquelyIdentifiable<TId>)} for this method to work.";
            InternalContract.Require(typeof(IUniquelyIdentifiable<TId>).IsAssignableFrom(typeof(TModel)), errorMessage);
            var items = new PageEnvelopeEnumerableAsync<TModel>((o, t) => ReadChildrenWithPagingAsync(masterId, o, null, t), token);
            var enumerator = items.GetEnumerator();
            while (await enumerator.MoveNextAsync())
            {
                if (!(enumerator.Current is IUniquelyIdentifiable<TId> item)) continue;
                await DeleteAsync(item.Id, token);
            }
        }

        /// <inheritdoc />
        public Task<PageEnvelope<TModel>> SearchChildrenAsync(TId parentId, SearchDetails<TModel> details, int offset, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            return _convenience.SearchChildrenAsync(parentId, details, offset, limit, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TModel> FindUniqueChildAsync(TId parentId, SearchDetails<TModel> details,
            CancellationToken cancellationToken = default)
        {
            return _convenience.FindUniqueChildAsync(parentId, details, cancellationToken);
        }

        /// <inheritdoc />
        public Task<TId> CreateChildAsync(TId parentId, TModelCreate item, CancellationToken token = default)
        {
            return _convenience.CreateChildAsync(parentId, item, token);
        }

        /// <inheritdoc />
        public Task<TModel> CreateChildAndReturnAsync(TId parentId, TModelCreate item, CancellationToken token = default)
        {
            return _convenience.CreateChildAndReturnAsync(parentId, item, token);
        }
    }
}
