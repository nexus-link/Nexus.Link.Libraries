﻿using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.Crud
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModelCreate,TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestICrudEtag<TId> : TestICrdEtag<TId>
    {
        protected override ICrud<TestItemBare, TestItemEtag<TId>, TId> CrdStorage => CrudStorage;

        /// <summary>
        /// Create an item with an id.
        /// </summary>
        [TestMethod]
        public async Task Update_Read_Etag_Async()
        {
            var id = await CreateItemAsync(TypeOfTestDataEnum.Variant1);
            var updateItem = await UpdateAndReturnItemAsync(id, TypeOfTestDataEnum.Variant2);
            Assert.IsNotNull(updateItem);
            var readItem = await ReadItemAsync(id);
            Assert.IsNotNull(readItem);
            if (!updateItem.Equals(readItem))
            {
                updateItem.Etag = readItem.Etag;
            }
            Assert.AreEqual(updateItem, readItem);
        }
    }
}

