using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{
    /// <summary>
    /// General class for storing a many to one item in memory.
    /// </summary>
    /// <typeparam name="TManyToManyModel">The model for many-to-many-relation.</typeparam>
    /// <typeparam name="TReferenceModel1">The first model of references.</typeparam>
    /// <typeparam name="TReferenceModel2">The second model of references.</typeparam>
    /// <typeparam name="TId">The type for the id field of the models.</typeparam>
    public class ManyToManyMemory<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId> :
        ManyToManyMemory<TManyToManyModel, TManyToManyModel, TReferenceModel1, TReferenceModel1, TReferenceModel2, TReferenceModel2, TId>,
        ICrudManyToMany<TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getForeignKey1Delegate">See <see cref="ManyToManyMemory{TManyToManyModelCreate,TManyToManyModel,TReferenceModel1Create,TReferenceModel1,TReferenceModel2Create,TReferenceModel2,TId}.GetForeignKeyDelegate"/>.</param>
        /// <param name="getForeignKey2Delegate">See <see cref="ManyToManyMemory{TManyToManyModelCreate,TManyToManyModel,TReferenceModel1Create,TReferenceModel1,TReferenceModel2Create,TReferenceModel2,TId}.GetForeignKeyDelegate"/>.</param>
        /// <param name="foreignHandler1">Functionality to read a specified parent.</param>
        /// <param name="foreignHandler2">Functionality to read a specified parent.</param>
        public ManyToManyMemory(GetForeignKeyDelegate getForeignKey1Delegate, GetForeignKeyDelegate getForeignKey2Delegate, ICrud<TReferenceModel1, TId> foreignHandler1, ICrud<TReferenceModel2, TId> foreignHandler2)
        : base(getForeignKey1Delegate, getForeignKey2Delegate, foreignHandler1, foreignHandler2)
        {
        }
    }

    /// <summary>
    /// General class for storing a many to one item in memory.
    /// </summary>
    /// <typeparam name="TManyToManyModel">The model for many-to-many-relation.</typeparam>
    /// <typeparam name="TReferenceModel1">The first model of references.</typeparam>
    /// <typeparam name="TReferenceModel2">The second model of references.</typeparam>
    /// <typeparam name="TId">The type for the id field of the models.</typeparam>
    /// <typeparam name="TReferenceModel2Create"></typeparam>
    /// <typeparam name="TReferenceModel1Create"></typeparam>
    /// <typeparam name="TManyToManyModelCreate"></typeparam>
    public class ManyToManyMemory<TManyToManyModelCreate, TManyToManyModel, TReferenceModel1Create, TReferenceModel1, TReferenceModel2Create, TReferenceModel2, TId> :
        CrudMemory<TManyToManyModelCreate, TManyToManyModel, TId>,
        ICrudManyToMany<TManyToManyModelCreate, TManyToManyModel, TReferenceModel1, TReferenceModel2, TId>
        where TManyToManyModel : TManyToManyModelCreate
        where TReferenceModel1 : TReferenceModel1Create
        where TReferenceModel2 : TReferenceModel2Create
    {
        private readonly GetForeignKeyDelegate _getForeignKey1Delegate;
        private readonly GetForeignKeyDelegate _getForeignKey2Delegate;
        private readonly ICrud<TReferenceModel1Create, TReferenceModel1, TId> _foreignHandler1;
        private readonly ICrud<TReferenceModel2Create, TReferenceModel2, TId> _foreignHandler2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="getForeignKey1Delegate">See <see cref="GetForeignKeyDelegate"/>.</param>
        /// <param name="getForeignKey2Delegate">See <see cref="GetForeignKeyDelegate"/>.</param>
        /// <param name="foreignHandler1">Functionality to read a specified parent.</param>
        /// <param name="foreignHandler2">Functionality to read a specified parent.</param>
        public ManyToManyMemory(GetForeignKeyDelegate getForeignKey1Delegate, GetForeignKeyDelegate getForeignKey2Delegate, ICrud<TReferenceModel1Create, TReferenceModel1, TId> foreignHandler1, ICrud<TReferenceModel2Create, TReferenceModel2, TId> foreignHandler2)
        {
            InternalContract.RequireNotNull(getForeignKey2Delegate, nameof(getForeignKey1Delegate));
            InternalContract.RequireNotNull(getForeignKey1Delegate, nameof(getForeignKey2Delegate));
            InternalContract.RequireNotNull(foreignHandler1, nameof(foreignHandler1));
            InternalContract.RequireNotNull(foreignHandler2, nameof(foreignHandler2));

            _getForeignKey1Delegate = getForeignKey1Delegate;
            _getForeignKey2Delegate = getForeignKey2Delegate;
            _foreignHandler1 = foreignHandler1;
            _foreignHandler2 = foreignHandler2;
        }

        /// <summary>
        /// A delegate method for getting a foreign key id from a stored item.
        /// </summary>
        /// <param name="item">The item to get the parent for.</param>
        public delegate TId GetForeignKeyDelegate(TManyToManyModel item);

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TReferenceModel2>> ReadReferencedItemsByReference1WithPagingAsync(TId id, int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            return await ReadReferencedItemsByForeignKeyAsync(
                id,
                _getForeignKey1Delegate,
                _getForeignKey2Delegate,
                _foreignHandler2,
                offset, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TReferenceModel2>> ReadReferencedItemsByReference1Async(TId id, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadReferencedItemsByReference1WithPagingAsync(id, offset, null, t), limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TReferenceModel1>> ReadReferencedItemsByReference2WithPagingAsync(TId id, int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            return await ReadReferencedItemsByForeignKeyAsync(
                id,
                _getForeignKey2Delegate,
                _getForeignKey1Delegate,
                _foreignHandler1,
                offset, limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TReferenceModel1>> ReadReferencedItemsByReference2Async(TId id, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadReferencedItemsByReference2WithPagingAsync(id, offset, null, t), limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task DeleteReferencedItemsByReference1(TId id, CancellationToken cancellationToken  = default)
        {
            await DeleteReferencedItemsByForeignKey<TReferenceModel2>(id, _getForeignKey1Delegate, _foreignHandler2, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task DeleteReferencedItemsByReference2(TId id, CancellationToken cancellationToken  = default)
        {
            await DeleteReferencedItemsByForeignKey<TReferenceModel1>(id, _getForeignKey2Delegate, _foreignHandler1, cancellationToken );
        }

        private Task<PageEnvelope<T>> ReadReferencedItemsByForeignKeyAsync<T>(TId id, GetForeignKeyDelegate idDelegate, GetForeignKeyDelegate referenceIdDelegate, IRead<T, TId> referenceHandler, int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            throw new FulcrumNotImplementedException("This method needs to be changed and tests");
            //limit = limit ?? PageInfo.DefaultLimit;
            //InternalContract.RequireNotNull(id, nameof(id));
            //InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            //InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            //List<Task<T>> taskList;
            //lock (MemoryItems)
            //{
            //    taskList = MemoryItems.Values
            //        .Where(i => id.Equals(idDelegate(i)))
            //        .Skip(offset)
            //        .Take(limit.Value)
            //        .Select(i => referenceHandler.ReadAsync(referenceIdDelegate(i), cancellationToken ))
            //        .ToList();
            //}

            //await Task.WhenAll(taskList);
            //var list = new List<T>();
            //foreach (var task in taskList)
            //{
            //    list.Add(await task);
            //}
            //var page = new PageEnvelope<T>(offset, limit.Value, null, list);
            //return await Task.FromResult(page);
        }

        private Task DeleteReferencedItemsByForeignKey<T>(TId id, GetForeignKeyDelegate idDelegate, IDelete<TId> referenceHandler, CancellationToken cancellationToken )
        {
            throw new FulcrumNotImplementedException("This method needs to be changed and tests");
            //InternalContract.RequireNotNull(id, nameof(id));
            //var errorMessage = $"{typeof(TManyToManyModel).Name} must implement the interface {nameof(IUniquelyIdentifiable<TId>)} for this method to work.";
            //InternalContract.Require(typeof(IUniquelyIdentifiable<TId>).IsAssignableFrom(typeof(TManyToManyModel)), errorMessage);
            //List<TManyToManyModel> list;
            //lock (MemoryItems)
            //{
            //    list = MemoryItems.Values
            //        .Where(i => id.Equals(idDelegate(i)))
            //        .ToList();
            //}

            //foreach (var item in list)
            //{
            //    if (!(item is IUniquelyIdentifiable<TId> idItem)) continue;
            //    await referenceHandler.DeleteAsync(idItem.Id, cancellationToken );
            //}
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TManyToManyModel>> ReadByReference1WithPagingAsync(TId reference1Id, int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(reference1Id, nameof(reference1Id));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            List<TManyToManyModel> list;
            lock (MemoryItems)
            {
                list = MemoryItems.Values
                    .Where(i => reference1Id.Equals(_getForeignKey1Delegate(i)))
                    .Skip(offset)
                    .Take(limit.Value)
                    .ToList();
            }
            var page = new PageEnvelope<TManyToManyModel>(offset, limit.Value, null, list);
            return await Task.FromResult(page);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TManyToManyModel>> ReadByReference1Async(TId reference1Id, int limit = int.MaxValue, CancellationToken cancellationToken  = default)
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadByReference1WithPagingAsync(reference1Id, offset, null, t), limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<PageEnvelope<TManyToManyModel>> ReadByReference2WithPagingAsync(TId reference2Id, int offset, int? limit = null, CancellationToken cancellationToken  = default)
        {
            limit = limit ?? PageInfo.DefaultLimit;
            InternalContract.RequireNotNull(reference2Id, nameof(reference2Id));
            InternalContract.RequireGreaterThanOrEqualTo(0, offset, nameof(offset));
            InternalContract.RequireGreaterThan(0, limit.Value, nameof(limit));
            List<TManyToManyModel> list;
            lock (MemoryItems)
            {
                list = MemoryItems.Values
                    .Where(i => reference2Id.Equals(_getForeignKey2Delegate(i)))
                    .Skip(offset)
                    .Take(limit.Value)
                    .ToList();
            }
            var page = new PageEnvelope<TManyToManyModel>(offset, limit.Value, null, list);
            return await Task.FromResult(page);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TManyToManyModel>> ReadByReference2Async(TId reference2Id, int limit = Int32.MaxValue, CancellationToken cancellationToken  = default)
        {
            return await StorageHelper.ReadPagesAsync((offset, t) => ReadByReference2WithPagingAsync(reference2Id, offset, null, t), limit, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task DeleteByReference1Async(TId reference1Id, CancellationToken cancellationToken  = default)
        {
            var enumerator = new PageEnvelopeEnumeratorAsync<TManyToManyModel>((offset, t) => ReadByReference1WithPagingAsync(reference1Id, offset, null, t), cancellationToken );
            await DeleteItemsAsync(enumerator, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task DeleteByReference2Async(TId reference2Id, CancellationToken cancellationToken  = default)
        {
            var enumerator = new PageEnvelopeEnumeratorAsync<TManyToManyModel>((offset, t) => ReadByReference2WithPagingAsync(reference2Id, offset, null, t), cancellationToken );
            await DeleteItemsAsync(enumerator, cancellationToken );
        }

        private async Task DeleteItemsAsync(PageEnvelopeEnumeratorAsync<TManyToManyModel> enumerator, CancellationToken cancellationToken )
        {
            var tasks = new List<Task>();
            while (await enumerator.MoveNextAsync())
            {
                var item = enumerator.Current;
                var itemWithId = item as IUniquelyIdentifiable<TId>;
                InternalContract.Require(itemWithId != null,
                    $"The type {typeof(TManyToManyModel).FullName} must implement {typeof(IUniquelyIdentifiable<TId>).Name} for this method to work.");
                if (itemWithId == null) break;
                var task = DeleteAsync(itemWithId.Id, cancellationToken );
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public virtual Task<TManyToManyModel> ReadAsync(TId masterId, TId slaveId, CancellationToken cancellationToken  = default)
        {
            TManyToManyModel item;
            lock (MemoryItems)
            {
                item = MemoryItems.Values
                    .FirstOrDefault(i =>
                        Equals(_getForeignKey1Delegate(i), masterId) && Equals(_getForeignKey2Delegate(i), slaveId));
            }

            return Task.FromResult(item);
        }

        /// <inheritdoc />
        public virtual async Task UpdateAsync(TId masterId, TId slaveId, TManyToManyModel item,
            CancellationToken cancellationToken  = default)
        {
            var id = await GetItemId(masterId, slaveId, cancellationToken );
            if (Equals(id, default(TId))) throw new FulcrumNotFoundException($"No item was found with reference id 1 = {masterId} and reference id 2 = {slaveId}.");
            await UpdateAsync(id, item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task DeleteAsync(TId masterId, TId slaveId, CancellationToken cancellationToken  = default)
        {
            var id = await GetItemId(masterId, slaveId, cancellationToken );
            if (Equals(id, default(TId))) return;
            await DeleteAsync(id, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task CreateWithSpecifiedIdAsync(TId masterId, TId slaveId, TManyToManyModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            return CreateAsync(item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual Task<TManyToManyModel> CreateWithSpecifiedIdAndReturnAsync(TId masterId, TId slaveId, TManyToManyModelCreate item,
            CancellationToken cancellationToken  = default)
        {
            return CreateAndReturnAsync(item, cancellationToken );
        }

        /// <inheritdoc />
        public virtual async Task<TManyToManyModel> UpdateAndReturnAsync(TId masterId, TId slaveId, TManyToManyModel item,
            CancellationToken cancellationToken  = default)
        {
            var id = await GetItemId(masterId, slaveId, cancellationToken );
            if (Equals(id, default(TId))) return default;
            return await UpdateAndReturnAsync(id, item, cancellationToken );
        }

        private async Task<TId> GetItemId(TId masterId, TId slaveId, CancellationToken cancellationToken )
        {
            var item = await ReadAsync(masterId, slaveId, cancellationToken );
            if (item == null) return default;
            var itemWithId = item as IUniquelyIdentifiable<TId>;
            InternalContract.Require(itemWithId != null,
                $"The type {typeof(TManyToManyModel).FullName} must implement {typeof(IUniquelyIdentifiable<TId>).Name} for this method to work.");
            if (itemWithId == null) return default;
            return itemWithId.Id;
        }
    }
}
