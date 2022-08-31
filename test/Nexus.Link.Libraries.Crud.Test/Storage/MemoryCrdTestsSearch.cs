using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.Test.Storage
{
    [TestClass]
    public class MemoryCrdTestsSearch : TestICrdSearch<Guid>
    {
        private ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            _storage = new CrudMemory<TestItemSort<Guid>, TestItemSort<Guid>, Guid>();
        }

        protected override ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> CrdStorage => _storage;

        protected override ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> CrudStorage => _storage;
    }
}
