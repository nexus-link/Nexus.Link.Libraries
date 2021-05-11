using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Crud
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestICrudSearch<TId>: TestICrdBase<TestItemBare, TestItemBare, TId>
    {
        [DataTestMethod]
        [DataRow("Variant1")]
        [DataRow("Variant2")]
        public async Task Find_Item_Async(string value)
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemBare, TId>;
            Assert.IsNotNull(search);
            var page = await search.SearchAsync(JToken.FromObject( new {Value = value}), null, 0, 10);

            // Assert
            Assert.AreEqual(1, page.PageInfo.Returned);
            Assert.AreEqual(value, page.Data.First().Value);
        }

        [DataTestMethod]
        [DataRow("Variant1")]
        [DataRow("Variant2")]
        public async Task Find_Duplicate_Item_Async(string value)
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemBare, TId>;
            Assert.IsNotNull(search);
            var page = await search.SearchAsync(JToken.FromObject( new {Value = value}), null, 0, 10);

            // Assert
            Assert.AreEqual(2, page.PageInfo.Returned);
        }

        [DataTestMethod]
        [DataRow("A")]
        [DataRow("B")]
        public async Task Dont_Find_Item_Async(string value)
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemBare, TId>;
            Assert.IsNotNull(search);
            var page = await search.SearchAsync(JToken.FromObject( new {Value = value}), null, 0, 10);

            // Assert
            Assert.AreEqual(0, page.PageInfo.Returned);
        }
    }
}


