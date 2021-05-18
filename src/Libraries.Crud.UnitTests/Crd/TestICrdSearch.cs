using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Model;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Crd
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestICrdSearch<TId> : TestICrdBase<TestItemSort<TId>, TestItemSort<TId>, TId>
    {
        [DataTestMethod]
        [DataRow("Variant1")]
        [DataRow("Variant2")]
        public async Task Search_Item_Async(string value)
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchAsync(new SearchDetails<TestItemSort<TId>>(new { Value = value }), 0, 10);

            // Assert
            Assert.AreEqual(1, page.PageInfo.Returned);
            foreach (var item in page.Data)
            {
                Assert.AreEqual(value, item.Value);
            }
        }

        [DataTestMethod]
        [DataRow("Var*", 2)]
        [DataRow("Def*", 1)]
        [DataRow("Variant?", 2)]
        public async Task SearchWithWildCard_Async(string value, int expectedFound)
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Default);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchAsync(new SearchDetails<TestItemSort<TId>>(new { Value = value }), 0, 10);

            // Assert
            Assert.AreEqual(expectedFound, page.PageInfo.Returned);
            foreach (var item in page.Data)
            {
                Assert.AreEqual(value.Substring(0,1), item.Value.Substring(0,1));
            }
        }

        [TestMethod]
        public async Task Find_SearchAll_Async()
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            var page = await search.SearchAsync(new SearchDetails<TestItemSort<TId>>(null), 0, 10);

            // Assert
            Assert.AreEqual(4, page.PageInfo.Returned);
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
            var page = await search.SearchAsync(new SearchDetails<TestItemSort<TId>>(new { Value = value }), 0, 10);

            // Assert
            Assert.AreEqual(2, page.PageInfo.Returned);
            foreach (var item in page.Data)
            {
                Assert.AreEqual(value, item.Value);
            }
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
                var searchDetails = new SearchDetails<TestItemSort<TId>>(new { Value = value }, new { IncreasingNumber = ascending });
                page = await search.SearchAsync(searchDetails, 0, 10);
            }
            else
            {
                var searchDetails = new SearchDetails<TestItemSort<TId>>(new { Value = value }, new { DecreasingString = @ascending });
                page = await search.SearchAsync(searchDetails, 0, 10);
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
            await CreateItemAsync(TypeOfTestDataEnum.Variant1); // 1, 1
            await CreateItemAsync(TypeOfTestDataEnum.Variant2); // 2, 0
            await CreateItemAsync(TypeOfTestDataEnum.Variant2); // 3, 1
            await CreateItemAsync(TypeOfTestDataEnum.Variant2); // 4, 0
            await CreateItemAsync(TypeOfTestDataEnum.Variant2); // 5, 1

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");

            var searchDetails = new SearchDetails<TestItemSort<TId>>(new { Value = "Variant2" }, new { NumberModulo = true, IncreasingNumber = false});
            var page = await search.SearchAsync(searchDetails, 0, 10);
            
            // Assert
            Assert.AreEqual(4, page.PageInfo.Returned);
            var array = page.Data.ToArray();
            Assert.AreEqual(0, array[0].NumberModulo);
            Assert.AreEqual(4, array[0].IncreasingNumber);
            Assert.AreEqual(0, array[1].NumberModulo);
            Assert.AreEqual(2, array[1].IncreasingNumber);
            Assert.AreEqual(1, array[2].NumberModulo);
            Assert.AreEqual(5, array[2].IncreasingNumber);
            Assert.AreEqual(1, array[3].NumberModulo);
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
            var page = await search.SearchAsync(new SearchDetails<TestItemSort<TId>>(new { Value = value }), 0, 10);

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
            var page = await search.SearchAsync(new SearchDetails<TestItemSort<TId>>(new { UnknownProperty = "56" }), 0, 10);

            // Assert
            Assert.AreEqual(0, page.PageInfo.Returned);
        }

        [TestMethod]
        public async Task FindUnique_NotFound_Async()
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            const string variant = "Variant1";
            var searchDetails = new SearchDetails<TestItemSort<TId>>(new { Value = variant });

            var item = await search.FindUniqueAsync(searchDetails);

            // Assert
            Assert.IsNull(item);

        }

        [TestMethod]
        public async Task FindUnique_Async()
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            const string variant = "Variant1";
            var searchDetails = new SearchDetails<TestItemSort<TId>>(new { Value = variant });

            var item = await search.FindUniqueAsync(searchDetails);

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(variant, item.Value);

        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumBusinessRuleException))]
        public async Task FindUnique_WithTooManyFound_Async()
        {
            // Arrange
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);
            await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            await CreateItemAsync(TypeOfTestDataEnum.Variant2);

            // Act 
            var search = CrudStorage as ISearch<TestItemSort<TId>, TId>;
            Assert.IsNotNull(search, $"{CrudStorage.GetType().Name} was expected to implement {nameof(ISearch<TestItemSort<TId>, TId>)}");
            const string variant = "Variant2";
            var searchDetails = new SearchDetails<TestItemSort<TId>>(new { Value = variant });

            var item = await search.FindUniqueAsync(searchDetails);

            // Assert
            Assert.IsNotNull(item);
            Assert.AreEqual(variant, item.Value);

        }
    }
}


