using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.ManyToOne
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestIManyToOneSearch<TId, TReferenceId> : TestIManyToOneBase<TId, TReferenceId> 
        
    { 
        [DataTestMethod]
        [DataRow("Variant1")]
        [DataRow("Variant2")]
        public async Task Find_Item_Async(string value)
        {
            // Arrange
            var parent = await CreateItemAsync(TypeOfTestDataEnum.Default);
            await CreateItemAsync(CrudManyStorageNonRecursive, TypeOfTestDataEnum.Variant1, parent.Id);
            await CreateItemAsync(CrudManyStorageNonRecursive, TypeOfTestDataEnum.Variant2, parent.Id);

            // Act 
            var search = CrudManyStorageNonRecursive as ISearchChildren<TestItemManyToOne<TId, TReferenceId>, TId>;
            Assert.IsNotNull(search, $"{CrudManyStorageNonRecursive.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchChildrenAsync(parent.Id, new SearchDetails<TestItemManyToOne<TId, TReferenceId>> { Where = new { Value = value }}, 0, 10);

            // Assert
            Assert.AreEqual(1, page.PageInfo.Returned);
            foreach (var item in page.Data)
            {
                Assert.AreEqual(value, item.Value);
            }
        }
    }
}

