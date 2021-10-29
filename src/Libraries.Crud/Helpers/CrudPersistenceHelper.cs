using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.Helpers
{
    /// <summary>
    /// Interface for the service in the <see cref="CrudPersistenceHelper{TModel,TId}"/> constructor.
    /// </summary>
    public interface ICrudPersistentHelperService<TModel, in TId> : IRead<TModel, TId>, IUpdateAndReturn<TModel, TId>,
        ICreateWithSpecifiedIdAndReturn<TModel, TId>
        where TModel : class, IOptimisticConcurrencyControlByETag
    {

    }

    /// <summary>
    /// A class that handles the logic around creating and updating 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class CrudPersistenceHelper<TModel, TId> : IRead<TModel, TId>
        where TModel : class, IOptimisticConcurrencyControlByETag
    {
        private readonly ICrudPersistentHelperService<TModel, TId> _crudService;
        private readonly CrudPersistenceHelperOptions _options;
        private readonly ConcurrentDictionary<TId, TModel> _stored = new ConcurrentDictionary<TId, TModel>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crudService">The persistence service that will actually create, read and update data.</param>
        /// <param name="options"></param>
        public CrudPersistenceHelper(ICrudPersistentHelperService<TModel, TId> crudService, CrudPersistenceHelperOptions options)
        {
            _crudService = crudService;
            _options = options;
        }

        /// <summary>
        /// Returns the item uniquely identified by <paramref name="id"/> from storage.
        /// The class will remember its values, so we know how to deal with a later call to <see cref="CreateOrUpdateAndReturnAsync"/>.
        /// </summary>
        /// <returns>The found item or null.</returns>
        public async Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken = default)
        {
            var item = await _crudService.ReadAsync(id, cancellationToken);
            if (item == null) return null;
            AddOrUpdateWithCopy(id, item);
            return item;
        }

        public Task ForgetAsync(TId id, CancellationToken cancellationToken = default)
        {
            _stored.TryRemove(id, out var _);
            return Task.CompletedTask;
        }

        public Task ForgetAllAsync(CancellationToken cancellationToken = default)
        {
            _stored.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Create or update the <paramref name="item"/>. If there is a conflict, use the strategy from the <see cref="CrudPersistenceHelperOptions"/>.
        /// </summary>
        /// <param name="id">How the object to be updated is identified.</param>
        /// <param name="item">The new version of the item. </param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>The updated item as it was saved - or the currently saved item if there was a conflict and the strategy says that we should read.</returns>
        // <exception cref="FulcrumConflictException">
        // Thrown if the same item already existed
        // </exception>
        public async Task<TModel> CreateOrUpdateAndReturnAsync(TId id, TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));

            try
            {
                if (item.Etag == null)
                {
                    if (_stored.ContainsKey(id))
                    {
                        throw new FulcrumConflictException(
                        $"{nameof(item.Etag)} == null which should indicate that this is a new item, but it has previously been successfully read from persistence.");
                    }

                    if (item is IUniquelyIdentifiable<TId> uniquelyIdentifiable)
                    {
                        uniquelyIdentifiable.Id = id;
                    }
                    item.Etag = "ignore";
                    var result = await _crudService.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
                    AddOrUpdateWithCopy(id, result);
                    return result;
                }
                else
                {
                    if (!HasChanged(id, item)) return item;
                    var result = await _crudService.UpdateAndReturnAsync(id, item, cancellationToken);
                    AddOrUpdateWithCopy(id, result);
                    return result;
                }
            }
            catch (FulcrumContractException)
            {
                switch (_options.ConflictStrategy)
                {
                    case PersistenceConflictStrategyEnum.Throw:
                        throw;
                    case PersistenceConflictStrategyEnum.ReturnNew:
                        var result = await _crudService.ReadAsync(id, cancellationToken);
                        if (result == null) return null;
                        AddOrUpdateWithCopy(id, result);
                        return result;
                    default:
                        FulcrumAssert.Fail($"Unexpected enum value: {_options.ConflictStrategy}");
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void AddOrUpdateWithCopy(TId id, TModel item)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            var copy = item.AsCopy();
            _stored.AddOrUpdate(id, i => copy, (i, oldValue) => copy);
        }

        private bool HasChanged(TId id, TModel item)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            var found = _stored.TryGetValue(id, out var stored);
            FulcrumAssert.IsTrue(found, CodeLocation.AsString());
            FulcrumAssert.IsNotNull(stored, CodeLocation.AsString());
            var etag = item.Etag;
            item.Etag = stored?.Etag;
            var equal = !JToken.DeepEquals(JToken.FromObject(stored), JToken.FromObject(item));
            item.Etag = etag;
            return !equal;
        }
    }

    public enum PersistenceConflictStrategyEnum
    {
        Throw,
        ReturnNew
    };

    public class CrudPersistenceHelperOptions
    {
        public PersistenceConflictStrategyEnum ConflictStrategy { get; set; } = PersistenceConflictStrategyEnum.Throw;
    }
}
