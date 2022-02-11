using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.Test.Storage
{
    [TestClass]
    public class MemoryCrudTestsId : TestICrudId<Guid>
    {
        private ICrud<TestItemBare, TestItemId<Guid>, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            _storage = new CrudMemory<TestItemBare, TestItemId<Guid>, Guid>();
        }

        protected override ICrud<TestItemBare, TestItemId<Guid>, Guid> CrudStorage => _storage;

        [Ignore]
        public new Task ClaimDistributedLock()
        {
            return Task.CompletedTask;
        }

        [Ignore]
        public new Task ClaimDistributedLock_Given_AlreadyLocked_GivesException()
        {
            return Task.CompletedTask;
        }

        [Ignore]
        public new Task ClaimDistributedLock_Release_Claim()
        {
            return Task.CompletedTask;
        }

        [Ignore]
        public new Task ClaimDistributedLock_Reclaim()
        {
            return Task.CompletedTask;
        }
    }
}
