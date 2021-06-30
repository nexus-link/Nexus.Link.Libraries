using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Crd
{
    public abstract class TestICrdBase<TModelCreate, TModel, TId>
        where TModelCreate : IItemForTesting, new()
    where TModel : TModelCreate
    {
        /// <summary>
        /// The storage that should be tested
        /// </summary>
        protected abstract ICrud<TModelCreate, TModel, TId> CrdStorage { get; }

        /// <summary>
        /// The storage that should be tested
        /// </summary>
        protected abstract ICrud<TModelCreate, TModel, TId> CrudStorage { get; }

        protected async Task<TId> CreateItemAsync(TypeOfTestDataEnum type, CancellationToken cancellationToken = default)
        {
            var initialItem = new TModelCreate();
            initialItem.InitializeWithDataForTesting(type);
            var id = await CrdStorage.CreateAsync(initialItem, cancellationToken);
            Assert.AreNotEqual(default, id);
            return id;
        }

        protected async Task<TModel> ReadItemAsync(TId id, CancellationToken cancellationToken = default)
        {
            var readItem = await CrdStorage.ReadAsync(id, cancellationToken);
            Assert.IsNotNull(readItem);
            return readItem;
        }

        protected async Task<TModel> UpdateItemAsync(TId id, TypeOfTestDataEnum type, CancellationToken cancellationToken = default)
        {
            var updatedItem = await ReadItemAsync(id, cancellationToken);
            updatedItem.InitializeWithDataForTesting(type);
            if (updatedItem is IUniquelyIdentifiable<TId> itemWithId)
            {
                itemWithId.Id = id;
            }
            await CrudStorage.UpdateAsync(id, updatedItem, cancellationToken);
            return await ReadItemAsync(id, cancellationToken);
        }
    }
}