using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    /// A class that handles the logic around creating and updating 
    /// </summary>
    public class CrudPersistenceHelper<TModelCreate, TModel, TId> : IRead<TModel, TId>
        where TModel : class, TModelCreate, IOptimisticConcurrencyControlByETag, IUniquelyIdentifiable<TId>
    {
        private readonly ICrudable<TModelCreate, TModel, TId> _crudService;
        private readonly IRead<TModel, TId> _readService;
        private readonly CrudPersistenceHelperOptions _options;
        private readonly ConcurrentDictionary<TId, TModel> _stored = new ConcurrentDictionary<TId, TModel>();
        private readonly List<TId> _currentInOrder = new List<TId>();
        private readonly ConcurrentDictionary<TId, TModel> _currentDictionary = new ConcurrentDictionary<TId, TModel>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crudService">The persistence service that will actually create, read and update data.</param>
        /// <param name="options"></param>
        public CrudPersistenceHelper(ICrudable<TModelCreate, TModel, TId> crudService, CrudPersistenceHelperOptions options)
        {
            _readService = crudService as IRead<TModel, TId>;
            InternalContract.Require(_readService != null, $"Parameter {nameof(crudService)} must implement {nameof(IRead<TModel, TId>)}.");
            _crudService = crudService;
            _options = options;
        }

        /// <summary>
        /// Returns the item uniquely identified by <paramref name="id"/> from storage.
        /// The class will remember its values, so we know how to deal with a later call to <see cref="SaveAsync(TId,TModel,System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns>The found item or null.</returns>
        public async Task<TModel> ReadAsync(TId id, CancellationToken cancellationToken = default)
        {
            var item = await _readService.ReadAsync(id, cancellationToken);
            if (item == null) return null;
            AddOrUpdateWithCopy(id, item, true);
            return item;
        }

        /// <summary>
        /// The item has already been initialized somehow, for instance by reading it yourself.
        /// The class will remember its values, so we know how to deal with a later call to <see cref="SaveAsync(TId,TModel,System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns>The found item or null.</returns>
        public void Add(TId id, TModel item, bool stored)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            AddOrUpdateWithCopy(id, item, stored);
        }

        public void Forget(TId id)
        {
            _stored.TryRemove(id, out var _);
            if (_currentDictionary.TryRemove(id, out var _))
            {
                _currentInOrder.Remove(id);
            }
        }

        public void ForgetAll()
        {
            _stored.Clear();
            _currentDictionary.Clear();
            _currentInOrder.Clear();
        }

        public TModel GetStored(TId id)
        {
            var found = _stored.TryGetValue(id, out var stored);
            if (!found || stored == null) return null;
            return stored.AsCopy();
        }

        public async Task SaveAsync(Func<TId, TModel, CancellationToken, Task> handleResultMethod, CancellationToken cancellationToken = default)
        {
            var itemTaskList = new List<Task<TModel>>();
            foreach (var id in _currentInOrder)
            {
                if (!_currentDictionary.TryGetValue(id, out var item)) continue;
                var itemTask = SaveAsync(id, item, cancellationToken);
                itemTaskList.Add(itemTask);
            }

            foreach (var itemTask in itemTaskList)
            {
                var item = await itemTask;
                await handleResultMethod(item.Id, item, cancellationToken);
            }
        }

        public Task SaveAsync(Action<TId, TModel> handleResultMethod, CancellationToken cancellationToken = default)
        {
            return SaveAsync((id, item, ct) =>
            {
                handleResultMethod(id, item);
                return Task.CompletedTask;
            }, cancellationToken);
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
        public async Task<TModel> SaveAsync(TId id, TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));

            try
            {
                var hasId = !Equals(id, default(TId));
                if (item.Etag == null)
                {
                    if (hasId && _stored.ContainsKey(id))
                    {
                        throw new FulcrumConflictException(
                        $"{nameof(item.Etag)} == null which should indicate that this is a new item, but it has previously been successfully read from persistence.");
                    }

                    if (hasId) item.Id = id;

                    item.Etag = "ignore";
                    TModel result = null;
                    if (_crudService is ICreateWithSpecifiedIdAndReturn<TModelCreate, TModel, TId> service1 && hasId)
                    {
                        result = await service1.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
                    }
                    else if (_crudService is ICreateWithSpecifiedIdNoReturn<TModelCreate, TModel, TId> service2 && hasId)
                    {
                        await service2.CreateWithSpecifiedIdAsync(id, item, cancellationToken);
                        result = await _readService.ReadAsync(id, cancellationToken);
                    }
                    else if (_crudService is ICreateAndReturn<TModelCreate, TModel, TId> service3 && !hasId)
                    {
                        result = await service3.CreateAndReturnAsync(item, cancellationToken);
                        id = result.Id;
                    }
                    else if (_crudService is ICreate<TModelCreate, TModel, TId> service4 && !hasId)
                    {
                        id = await service4.CreateAsync(item, cancellationToken);
                        result = await _readService.ReadAsync(id, cancellationToken);
                    }
                    else
                    {
                        InternalContract.Require(false,
                            "The CRUD service must implement"
                            + $" {nameof(ICreateWithSpecifiedIdAndReturn<TModelCreate, TModel, TId>)}"
                            + $" or {nameof(ICreateWithSpecifiedId<TModelCreate, TModel, TId>)}"
                            + $" or {nameof(ICreateAndReturn<TModelCreate, TModel, TId>)}"
                        + $" or {nameof(ICreate<TModelCreate, TModel, TId>)}");
                    }

                    AddOrUpdateWithCopy(id, result, true);
                    return result;
                }
                else
                {
                    if (!HasChanged(id, item)) return item;
                    TModel result = null;
                    if (_crudService is IUpdateAndReturn<TModel, TId> service1)
                    {
                        result = await service1.UpdateAndReturnAsync(id, item, cancellationToken);
                    }
                    else if (_crudService is IUpdate<TModel, TId> service2)
                    {
                        await service2.UpdateAsync(id, item, cancellationToken);
                        result = await _readService.ReadAsync(id, cancellationToken);
                    }
                    else
                    {
                        InternalContract.Require(false, $"The CRUD service must implement {nameof(IUpdateAndReturn<TModel, TId>)} or {nameof(IUpdate<TModel, TId>)}");
                    }
                    AddOrUpdateWithCopy(id, result, true);
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
                        var result = await _readService.ReadAsync(id, cancellationToken);
                        if (result == null) return null;
                        AddOrUpdateWithCopy(id, result, true);
                        return result;
                    default:
                        FulcrumAssert.Fail($"Unexpected enum value: {_options.ConflictStrategy}");
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public Task<TModel> SaveAsync(TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            TId id = default;
            if (item is IUniquelyIdentifiable<TId> uniquelyIdentifiable)
            {
                id = uniquelyIdentifiable.Id;
            }
            return SaveAsync(id, item, cancellationToken);
        }

        private void AddOrUpdateWithCopy(TId id, TModel item, bool copyAsStored)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            _currentDictionary.AddOrUpdate(id, i =>
            {
                _currentInOrder.Add(id);
                return item;
            }, (i, oldValue) => item);
            if (!copyAsStored) return;
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
            var equal = JToken.DeepEquals(JToken.FromObject(stored), JToken.FromObject(item));
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
