using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Queue.Logic;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Queue
{
    [TestClass]
    public class TestMemoryQueue
    {
        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestMemoryQueue).FullName);
        }

        // This test was supposed to verify that the logs run in parallel. We have removed that capability as long as the memory queue can't be run in parallel.
        [TestMethod]
        public void CanRunWithoutIndividualAwait()
        {
            var stopWatch = new Stopwatch();
            var queue = new MemoryQueue<string>(nameof(CanRunWithoutIndividualAwait), SlowItemAction, true);
            queue.KeepQueueAliveTimeSpan = TimeSpan.Zero;
            stopWatch.Start();
            for (var i = 0; i < 20000; i++)
            {
                queue.AddMessage($"item {i}");
            }

            UT.Assert.IsTrue(queue.OnlyForUnitTest_HasAliveBackgroundWorker);
            while (queue.OnlyForUnitTest_HasAliveBackgroundWorker)
            {
                Console.WriteLine($"LatestItemFetchedAfterActiveTimeSpan: {queue.LatestItemFetchedAfterActiveTimeSpan.TotalMilliseconds} milliseconds.");
                Thread.Sleep(TimeSpan.FromMilliseconds(20));
            }
            stopWatch.Stop();
            Console.WriteLine($"LatestItemFetchedAfterActiveTimeSpan: {queue.LatestItemFetchedAfterActiveTimeSpan.TotalMilliseconds} milliseconds.");
            Console.WriteLine();
            Console.WriteLine($"Total time: {stopWatch.Elapsed.TotalMilliseconds} milliseconds");
            UT.Assert.AreEqual(TimeSpan.Zero, queue.LatestItemFetchedAfterActiveTimeSpan);
            UT.Assert.IsTrue(stopWatch.ElapsedMilliseconds < 2000);
        }

        private static async Task SlowItemAction(string item)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}
