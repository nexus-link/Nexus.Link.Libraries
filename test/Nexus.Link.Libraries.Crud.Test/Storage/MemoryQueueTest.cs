using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Queue.Logic;
using Nexus.Link.Libraries.Core.Queue.Model;
using Nexus.Link.Libraries.Crud.Test.NuGet;
using Nexus.Link.Libraries.Crud.Test.NuGet.Model;

namespace Nexus.Link.Libraries.Crud.Test.Core.Storage
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
