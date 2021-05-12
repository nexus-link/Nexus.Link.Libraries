using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
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
    public abstract class TestICrudSearch<TId> : TestICrdBase<TestItemSort<TId>, TestItemSort<TId>, TId>
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
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchAsync(new { Value = value }, null, 0, 10);

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
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchAsync(new { Value = value }, null, 0, 10);

            // Assert
            Assert.AreEqual(2, page.PageInfo.Returned);
        }

        [DataTestMethod]
        //[DataRow("Variant1", true, true)]
        //[DataRow("Variant1", true, false)]
        //[DataRow("Variant1", false, true)]
        //[DataRow("Variant1", false, false)]
        [DataRow("Variant2", true, true)]
        [DataRow("Variant2", true, false)]
        [DataRow("Variant2", false, true)]
        [DataRow("Variant2", false, false)]
        public async Task SearchOrderBy_Async(string value, bool orderByNumber, bool ascending)
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            PageEnvelope<TestItemSort<TId>> page;
            if (orderByNumber)
            {
                page = await search.SearchAsync(new { Value = value }, new { IncreasingNumber = ascending }, 0, 10);
            }
            else
            {
                page = await search.SearchAsync(new { Value = value }, new { DecreasingString = ascending }, 0, 10);
            }



            // Assert
            Assert.AreEqual(2, page.PageInfo.Returned);
            var array = page.Data.ToArray();
            if (orderByNumber)
            {
                if (ascending)
                {
                    Assert.IsTrue(array[0].IncreasingNumber < array[1].IncreasingNumber);
                }
                else
                {
                    Assert.IsTrue(array[0].IncreasingNumber > array[1].IncreasingNumber);
                }
            }
            else
            {
                if (ascending)
                {
                    Assert.IsTrue(string.Compare(array[0].DecreasingString, array[1].DecreasingString, StringComparison.InvariantCulture) == -1);
                }
                else
                {
                    Assert.IsTrue(string.Compare(array[0].DecreasingString, array[1].DecreasingString, StringComparison.InvariantCulture) == 1);
                }
            }

        }

        [TestMethod]
        public async Task SearchNestedOrderBy_Async()
        {
            // Arrange
            TestItemBare.Modulo = 2;
            TestItemBare.Count = 1;
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");

            var page = await search.SearchAsync(new { Value = "Variant2" }, new { NumberModulo = true, IncreasingNumber = false}, 0, 10);
            
            // Assert
            Assert.AreEqual(4, page.PageInfo.Returned);
            var array = page.Data.ToArray();
            Assert.AreEqual(0, array[0].NumberModulo);
            Assert.AreEqual(0, array[1].NumberModulo);
            Assert.AreEqual(1, array[2].NumberModulo);
            Assert.AreEqual(1, array[3].NumberModulo);
            Assert.AreEqual(4, array[0].IncreasingNumber);
            Assert.AreEqual(2, array[1].IncreasingNumber);
            Assert.AreEqual(5, array[2].IncreasingNumber);
            Assert.AreEqual(3, array[3].IncreasingNumber);

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
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchAsync(new { Value = value }, null, 0, 10);

            // Assert
            Assert.AreEqual(0, page.PageInfo.Returned);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public async Task Unknown_Property_Async()
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchAsync(new { UnknownProperty = "56" }, null, 0, 10);

            // Assert
            Assert.AreEqual(0, page.PageInfo.Returned);
        }
    }
}


