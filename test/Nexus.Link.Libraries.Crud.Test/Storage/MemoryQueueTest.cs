using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Queue.Logic;
using Nexus.Link.Libraries.Core.Queue.Model;
using Nexus.Link.Libraries.Crud.UnitTests;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.Test.Storage
{
    [TestClass]
    public class MemoryQueueTest : TestIQueue
    {
        private MemoryQueue<TestItemBare> _queue;

        [TestInitialize]
        public void Inititalize()
        {
            _queue = new MemoryQueue<TestItemBare>("test-queue");
        }

        protected override ICompleteQueue<TestItemBare> Queue => _queue;
    }
}
