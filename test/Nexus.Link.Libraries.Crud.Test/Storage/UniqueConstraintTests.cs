using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Model;
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
            _storage.UniqueConstraintAsyncMethods += async (i, token) =>
            {
                var page = await _storage.SearchAsync(new SearchDetails<TestItemId<Guid>>(new { i.Value }), 0, 1,
                    token);
                if (page != null && page.PageInfo.Returned > 0)
                {
                    throw new FulcrumConflictException($"Item is not unique.");
                }
            };
            var item = new TestItemId<Guid>();
            item.InitializeWithDataForTesting(TypeOfTestDataEnum.Variant1);

            await _storage.CreateAsync(item);
            await _storage.CreateAsync(item);
        }
    }
}
