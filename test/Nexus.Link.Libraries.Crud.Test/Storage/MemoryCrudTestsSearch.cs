using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.Test.Storage
{
    [TestClass]
    public class MemoryCrudTestsSearch : TestICrudSearch<Guid>
    {
        private ICrud<TestItemBare, TestItemId<Guid>, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            _storage = new CrudMemory<TestItemBare, TestItemId<Guid>, Guid>();
        }

        protected override ICrud<TestItemBare, TestItemId<Guid>, Guid> CrdStorage => _storage;

        protected override ICrud<TestItemBare, TestItemId<Guid>, Guid> CrudStorage => _storage;
    }
}
