using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Test.NuGet.Crud;
using Nexus.Link.Libraries.Crud.Test.NuGet.Model;

namespace Nexus.Link.Libraries.Crud.Test.Core.Storage
{
    [TestClass]
    public class MemoryCrudTestsBare : TestICrudBare<Guid>
    {
        private ICrud<TestItemBare, TestItemBare, Guid> _storage;

        [TestInitialize]
        public void Inititalize()
        {
            _storage = new CrudMemory<TestItemBare, TestItemBare, Guid>();
        }

        protected override ICrud<TestItemBare, TestItemBare, Guid> CrudStorage => _storage;
    }
}
