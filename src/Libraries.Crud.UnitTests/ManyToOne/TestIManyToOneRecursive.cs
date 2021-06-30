using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.UnitTests.ManyToOne
{
    /// <summary>
    /// Tests for testing any storage that implements <see cref="ICrud{TModel,TId}"/>
    /// </summary>
    [TestClass]
    public abstract class TestIManyToOneRecursive<TId, TReferenceId> : TestIManyToOneBase<TId, TReferenceId>
    { 
        /// <summary>
        /// Create a recursive relation
        /// </summary>
        [TestMethod]
        public async Task SimpleRelationAsync()
        {
            var parent = await CreateItemAsync(CrudManyStorageRecursive, TypeOfTestDataEnum.Variant1, default(TReferenceId));
            var child = await CreateItemAsync(CrudManyStorageRecursive, TypeOfTestDataEnum.Variant2, parent.Id);
            Assert.IsNotNull(child.ParentId);
            Assert.AreNotEqual(default, child.ParentId);
            Assert.AreNotEqual(parent.Value, child.Value);
        }
    }
}

