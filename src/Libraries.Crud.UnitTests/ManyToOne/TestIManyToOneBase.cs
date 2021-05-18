using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.ManyToOne
{
    public abstract class TestIManyToOneBase<TId, TReferenceId>
    {
        /// <summary>
        /// The storage that should be tested
        /// </summary>
        protected abstract ICrudManyToOne<TestItemManyToOneCreate<TId, TReferenceId>, TestItemManyToOne<TId, TReferenceId>, TId>
            CrudManyStorageRecursive
        { get; }

        /// <summary>
        /// The storage that should be tested
        /// </summary>
        protected abstract
            ICrudManyToOne<TestItemManyToOneCreate<TId, TReferenceId>, TestItemManyToOne<TId, TReferenceId>, TId>
            CrudManyStorageNonRecursive
        { get; }

        /// <summary>
        /// The storage that should be tested
        /// </summary>
        protected abstract ICrud<TestItemId<TId>, TId> OneStorage { get; }

        protected async Task<TestItemManyToOne<TId, TReferenceId>> CreateItemAsync(
            ICrud<TestItemManyToOneCreate<TId, TReferenceId>, TestItemManyToOne<TId, TReferenceId>, TId> storage, TypeOfTestDataEnum type, TId parentId)
        {
            return await CreateItemAsync(storage, type,
                StorageHelper.ConvertToParameterType<TReferenceId>(parentId));
        }

        protected async Task<TestItemManyToOne<TId, TReferenceId>> CreateItemAsync(
                ICrud<TestItemManyToOneCreate<TId, TReferenceId>, TestItemManyToOne<TId, TReferenceId>, TId> storage, TypeOfTestDataEnum type, TReferenceId parentId)
        {
            var item = new TestItemManyToOneCreate<TId, TReferenceId>();
            item.InitializeWithDataForTesting(type);
            item.ParentId = parentId;
            var createdItem = await storage.CreateAndReturnAsync(item);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(default(TId), createdItem);
            return createdItem;
        }

        protected async Task<TestItemId<TId>> CreateItemAsync(TypeOfTestDataEnum type)
        {
            var item = new TestItemId<TId>();
            item.InitializeWithDataForTesting(type);
            var createdItem = await OneStorage.CreateAndReturnAsync(item);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(default(TId), createdItem);
            return createdItem;
        }
    }
}