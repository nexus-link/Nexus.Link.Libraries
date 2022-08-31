using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.Test.Storage
{
    [TestClass]
    public class UniqueConstraintTests
    {
        private CrudMemory<TestItemId<Guid>, TestItemId<Guid>, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            _storage = new CrudMemory<TestItemId<Guid>, TestItemId<Guid>, Guid>();
        }

        [TestMethod]
        public async Task Create_Given_NoConstraint_Gives_CanAddDuplicates()
        {
            var item = new TestItemId<Guid>();
            item.InitializeWithDataForTesting(TypeOfTestDataEnum.Variant1);

            await _storage.CreateAsync(item);
            await _storage.CreateAsync(item);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumConflictException))]
        public async Task Create_Given_Constraint_Gives_CantAddDuplicates()
        {
            _storage.UniqueConstraintMethods += i => new { i.Value };
            var item = new TestItemId<Guid>();
            item.InitializeWithDataForTesting(TypeOfTestDataEnum.Variant1);

            await _storage.CreateAsync(item);
            await _storage.CreateAsync(item);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumConflictException))]
        public async Task Update_Given_Constraint_Gives_CantAddDuplicates()
        {
            _storage.UniqueConstraintMethods += i => new { i.Value };
            var item1 = new TestItemId<Guid>();
            item1.InitializeWithDataForTesting(TypeOfTestDataEnum.Variant1);
            item1.Id = await _storage.CreateAsync(item1);

            var item2 = new TestItemId<Guid>();
            item2.InitializeWithDataForTesting(TypeOfTestDataEnum.Variant2);
            item2.Id = await _storage.CreateAsync(item2);

            item2.Value = item1.Value;
            await _storage.UpdateAsync(item2.Id, item2);

        }
    }
}
