using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
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
        private readonly IComparer<TModel> _saveOrderComparer;
        private readonly ConcurrentDictionary<TId, TModel> _stored = new ConcurrentDictionary<TId, TModel>();
        private readonly ConcurrentDictionary<TId, TModel> _transactionItems = new ConcurrentDictionary<TId, TModel>();
        private readonly List<TId> _currentInOrder = new List<TId>();
        private readonly ConcurrentDictionary<TId, TModel> _currentDictionary = new ConcurrentDictionary<TId, TModel>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crudService">The persistence service that will actually create, read and update data.</param>
        /// <param name="options"></param>
        /// <param name="saveOrderComparer">If this is not null, we will sort the items in this order before we save them.</param>
        public CrudPersistenceHelper(ICrudable<TModelCreate, TModel, TId> crudService,
            CrudPersistenceHelperOptions options, IComparer<TModel> saveOrderComparer = null)
        {
            _readService = crudService as IRead<TModel, TId>;
            InternalContract.Require(_readService != null,
                $"Parameter {nameof(crudService)} must implement {nameof(IRead<TModel, TId>)}.");
            _crudService = crudService;
            _options = options;
            _saveOrderComparer = saveOrderComparer;
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
            AddOrUpdateWithCopy(id, item, true, false);
            return item;
        }

        /// <summary>
        /// The item has already been initialized somehow, for instance by reading it yourself.
        /// The class will remember its values, so we know how to deal with a later call to <see cref="SaveAsync(TId,TModel,System.Threading.CancellationToken)"/>.
        /// </summary>
        /// <returns>The found item or null.</returns>
        public void Add(TId id, TModel item, bool considerAsStored)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            AddOrUpdateWithCopy(id, item, considerAsStored, false);
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

        public async Task SaveAsync(Func<TId, TModel, CancellationToken, Task> handleResultMethod,
            CancellationToken cancellationToken = default)
        {
            await PrivateSaveAllAsync(handleResultMethod, false, cancellationToken);
        }

        public async Task SaveAsTransactionAsync(CancellationToken cancellationToken = default)
        {
            await PrivateSaveAllAsync(null, true, cancellationToken);
        }

        private async Task PrivateSaveAllAsync(Func<TId, TModel, CancellationToken, Task> handleResultMethod, bool transaction, CancellationToken cancellationToken = default)
        {
            InternalContract.Require(handleResultMethod == null || !transaction, $"Can't have parameter {nameof(handleResultMethod)} when {nameof(transaction)} is true." +
                $" It should be used in the {nameof(CommitAsync)} method.");
            var itemTaskList = new List<Task<TModel>>();
            var items = _currentInOrder.Select(id =>
            {
                _currentDictionary.TryGetValue(id, out var item);
                return item;
            }).Where(item => item != null);
            var array = MaybeSort(items);
            Log.LogInformation($"Saving {array.Length} items.");
            for (int i = 0; i < array.Length; i++)
            {
                var item = array[i];
                if (item == null) continue;
                Log.LogVerbose($"Saving item {i+1} with id {item.Id}.");
                var itemTask = PrivateSaveItemAsync(item.Id, item, transaction, cancellationToken);
                if (_options.OnlySequential || _saveOrderComparer != null)
                {
                    // If the options says so, or if the save order is important, we can't use parallelism
                    await itemTask;
                }
                itemTaskList.Add(itemTask);
            }

            if (handleResultMethod == null)
            {
                await Task.WhenAll(itemTaskList);
            }
            else
            {
                foreach (var itemTask in itemTaskList)
                {
                    var item = await itemTask;
                    if (item == null) continue;
                    await handleResultMethod.Invoke(item.Id, item, cancellationToken);
                }
            }
        }

        private TModel[] MaybeSort(IEnumerable<TModel> items)
        {
            return _saveOrderComparer == null
                ? items.ToArray()
                : items.OrderBy(item => item, _saveOrderComparer).ToArray();
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
            return await PrivateSaveItemAsync(id, item, false, cancellationToken);
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
        public async Task<TModel> SaveAsTransactionAsync(TId id, TModel item, CancellationToken cancellationToken = default)
        {
            return await PrivateSaveItemAsync(id, item, true, cancellationToken);
        }

        /// <summary>
        /// Create or update the <paramref name="item"/>. If there is a conflict, use the strategy from the <see cref="CrudPersistenceHelperOptions"/>.
        /// </summary>
        /// <param name="id">How the object to be updated is identified.</param>
        /// <param name="item">The new version of the item. </param>
        /// <param name="transaction">True means that we should see the save as a transaction and the caller will call <see cref="Commit"/> to commit the transaction,
        /// or <see cref="Rollback"/>  to rollback the transaction.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled</param>
        /// <returns>The updated item as it was saved - or the currently saved item if there was a conflict and the strategy says that we should read.</returns>
        // <exception cref="FulcrumConflictException">
        // Thrown if the same item already existed
        // </exception>
        private async Task<TModel> PrivateSaveItemAsync(TId id, TModel item, bool transaction, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));

            try
            {
                var hasId = !Equals(id, default(TId));
                if (item.Etag == null && hasId && _stored.TryGetValue(id, out var storedValue))
                {
                    item.Etag = storedValue.Etag;
                }

                TModel createdOrUpdatedItem;
                if (item.Etag == null)
                {
                    createdOrUpdatedItem = await CreateInDatabaseAsync(hasId);
                }
                else
                {
                    createdOrUpdatedItem = await UpdateInDatabaseAsync();
                }
                return createdOrUpdatedItem;
            }
            catch (FulcrumConflictException ex)
            {
                Log.LogWarning($"Conflict for {id}, will use conflict strategy {_options.ConflictStrategy}", ex);
                switch (_options.ConflictStrategy)
                {
                    case PersistenceConflictStrategyEnum.Throw:
                        throw;
                    case PersistenceConflictStrategyEnum.ReturnNew:
                        var result = await _readService.ReadAsync(id, cancellationToken);
                        if (result == null) return null;
                        AddOrUpdateWithCopy(id, result, true, transaction);
                        return result;
                    default:
                        throw new FulcrumAssertionFailedException($"Unexpected enum value: {_options.ConflictStrategy}");
                }
            }

            async Task<TModel> CreateInDatabaseAsync(bool hasId)
            {
                try
                {
                    TModel createdItem;
                    item.Etag = "ignore";
                    if (hasId)
                    {
                        item.Id = id;
                        createdItem = await CreateWithSpecifiedIdAsync();
                    }
                    else
                    {
                        (id, createdItem) = await CreateWithNewIdAsync();
                    }
                    AddOrUpdateWithCopy(id, createdItem, true, transaction);
                    return createdItem;
                }
                finally
                {
                    item.Etag = null;
                }
            }

            async Task<TModel> UpdateInDatabaseAsync()
            {
                if (!HasChanged(id, item))
                {
                    Log.LogVerbose($"Already up to date: {id})");
                    return item;
                }
                var updatedItem = await UpdateAsync();

                AddOrUpdateWithCopy(id, updatedItem, true, transaction);
                return updatedItem;
            }

            async Task<TModel> CreateWithSpecifiedIdAsync()
            {
                switch (_crudService)
                {
                    case ICreateWithSpecifiedIdAndReturn<TModelCreate, TModel, TId>
                        cwsiarService:
                        {
                            var result = await cwsiarService.CreateWithSpecifiedIdAndReturnAsync(id, item, cancellationToken);
                            Log.LogVerbose($"Success: CreateWithSpecifiedIdAndReturnAsync({id})");
                            return result;
                        }
                    case ICreateWithSpecifiedIdNoReturn<TModelCreate, TModel, TId>
                        cwsinrService:
                        {
                            await cwsinrService.CreateWithSpecifiedIdAsync(id, item, cancellationToken);
                            Log.LogVerbose($"Success: CreateWithSpecifiedIdAsync({id})");
                            var result = await _readService.ReadAsync(id, cancellationToken);
                            Log.LogVerbose($"Success: ReadAsync({id})");
                            return result;
                        }
                    default:
                        {
                            const string message = "The CRUD service must implement"
                                                   + $" {nameof(ICreateWithSpecifiedIdAndReturn<TModelCreate, TModel, TId>)}"
                                                   + $" or {nameof(ICreateWithSpecifiedId<TModelCreate, TModel, TId>)}";
                            var fulcrumContractException = new FulcrumContractException(message);
                            Log.LogError(message, fulcrumContractException);
                            throw fulcrumContractException;
                        }
                }
            }

            async Task<(TId, TModel)> CreateWithNewIdAsync()
            {
                switch (_crudService)
                {
                    case ICreateAndReturn<TModelCreate, TModel, TId> carService:
                        {
                            var result = await carService.CreateAndReturnAsync(item, cancellationToken);
                            Log.LogVerbose($"Success: CreateAndReturnAsync() => {result.Id}");
                            id = result.Id;
                            return (id, result);
                        }
                    case ICreate<TModelCreate, TModel, TId> cService:
                        {
                            id = await cService.CreateAsync(item, cancellationToken);
                            Log.LogVerbose($"Success: CreateAsync() => {id}");
                            var result = await _readService.ReadAsync(id, cancellationToken);
                            Log.LogVerbose($"Success: ReadAsync({id})");
                            return (id, result);
                        }
                    default:
                        {
                            const string message = "The CRUD service must implement"
                                                   + $" {nameof(ICreateAndReturn<TModelCreate, TModel, TId>)}"
                                                   + $" or {nameof(ICreate<TModelCreate, TModel, TId>)}";
                            var fulcrumContractException = new FulcrumContractException(message);
                            Log.LogError(message, fulcrumContractException);
                            throw fulcrumContractException;
                        }
                }
            }

            async Task<TModel> UpdateAsync()
            {
                switch (_crudService)
                {
                    case IUpdateAndReturn<TModel, TId> service1:
                        {
                            var result = await service1.UpdateAndReturnAsync(id, item, cancellationToken);
                            Log.LogVerbose($"Success: UpdateAndReturnAsync({id})");
                            return result;
                        }
                    case IUpdate<TModel, TId> service2:
                        {
                            await service2.UpdateAsync(id, item, cancellationToken);
                            Log.LogVerbose($"Success: UpdateAsync({id})");
                            var result = await _readService.ReadAsync(id, cancellationToken);
                            Log.LogVerbose($"Success: ReadAsync({id})");
                            return result;
                        }
                    default:
                        {
                            const string message = $"The CRUD service must implement {nameof(IUpdateAndReturn<TModel, TId>)} or {nameof(IUpdate<TModel, TId>)}";
                            var fulcrumContractException = new FulcrumContractException(message);
                            Log.LogError(message, fulcrumContractException);
                            throw fulcrumContractException;
                        }
                }
            }
        }

        public Task<TModel> SaveAsync(TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            var id = item.GetPrimaryKey<TModel, TId>();
            return SaveAsync(id, item, cancellationToken);
        }

        public Task<TModel> SaveAsTransactionAsync(TModel item, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            var id = item.GetPrimaryKey<TModel, TId>();
            return PrivateSaveItemAsync(id, item, true, cancellationToken);
        }

        public async Task CommitAsync(Func<TId, TModel, CancellationToken, Task> handleResultMethod, CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            foreach (var valuePair in _transactionItems)
            {
                AddOrUpdateWithCopy(valuePair.Key, valuePair.Value, false, false);
            }

            if (handleResultMethod != null)
            {
                foreach (var valuePair in _transactionItems)
                {
                    await handleResultMethod.Invoke(valuePair.Key, valuePair.Value, cancellationToken);
                }
            }
            _transactionItems.Clear();
        }

        public void Commit(Action<TId, TModel> handleResultMethod)
        {
            foreach (var valuePair in _transactionItems)
            {
                AddOrUpdateWithCopy(valuePair.Key, valuePair.Value, true, false);
            }

            if (handleResultMethod != null)
            {
                foreach (var valuePair in _transactionItems)
                {
                    handleResultMethod.Invoke(valuePair.Key, valuePair.Value);
                }
            }
            _transactionItems.Clear();
        }

        public void Rollback()
        {
            _transactionItems.Clear();
        }

        private void AddOrUpdateWithCopy(TId id, TModel item, bool considerAsStored, bool transaction)
        {
            InternalContract.RequireNotNull(item, nameof(item));
            if (!transaction)
            {
                lock (_currentInOrder)
                {
                    _currentDictionary.AddOrUpdate(id, i =>
                    {
                        _currentInOrder.Add(id);
                        return item;
                    }, (i, oldValue) => item);
                }
            }

            if (!considerAsStored) return;
            var copy = item.AsCopy();
            if (transaction)
            {
                _transactionItems.AddOrUpdate(id, i => copy, (i, oldValue) => copy);
            }
            else
            {
                _stored.AddOrUpdate(id, i => copy, (i, oldValue) => copy);
            }
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

        public bool OnlySequential { get; set; }
    }
}