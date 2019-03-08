using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Test.NuGet.Crud;
using Nexus.Link.Libraries.Crud.Test.NuGet.Model;

namespace Nexus.Link.Libraries.Crud.Test.Core.Storage
{
    [TestClass]
    public class MemoryCrudTestsTimeStamped : TestICrudTimeStamped<Guid>
    {
        private ICrud<TestItemBare, TestItemTimestamped<Guid>, Guid> _storage;

        [TestInitialize]
        public void Inititalize()
        {
            _storage = new CrudMemory<TestItemBare, TestItemTimestamped<Guid>, Guid>();
        }

        protected override ICrud<TestItemBare, TestItemTimestamped<Guid>, Guid> CrudStorage => _storage;
    }
}
