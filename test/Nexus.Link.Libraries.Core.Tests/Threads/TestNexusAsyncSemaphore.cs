using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Threads;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Threads
{
    [TestClass]
    public class TestNexusAsyncSemaphore
    {
        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestThreadHelper).FullName);
        }

        [TestMethod]
        public Task VerifyNoWaitTimeAsync()
        {
            var done = false;
            var tokenSource = new CancellationTokenSource();
            var asyncSemaphore = new NexusAsyncSemaphore(TimeSpan.FromDays(1));
            var task = asyncSemaphore.ExecuteAsync(() =>
            {
                done = true;
                return Task.CompletedTask;
            }, tokenSource.Token);
            // 10ms grace period for ExecuteAloneAsync() to finish
            task.Wait(TimeSpan.FromMilliseconds(10));
            tokenSource.Cancel();
            UT.Assert.IsTrue(done, "It took too long time for the lambda method to be executed.");
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task RaiseAndLower()
        {
            var asyncSemaphore = new NexusAsyncSemaphore();
            await asyncSemaphore.RaiseAsync();
            asyncSemaphore.Lower();
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public void VerifyCannotLowerFirst()
        {
            var asyncSemaphore = new NexusAsyncSemaphore();
            asyncSemaphore.Lower();
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public async Task VerifyCannotLowerTwice()
        {
            var asyncSemaphore = new NexusAsyncSemaphore();
            await asyncSemaphore.RaiseAsync();
            asyncSemaphore.Lower();
            asyncSemaphore.Lower();
        }

        [TestMethod]
        public async Task VerifyNoParallelism()
        {
            var started1 = false;
            var done1 = false;
            var started2 = false;
            var asyncSemaphore = new NexusAsyncSemaphore();
            var task1 = asyncSemaphore.ExecuteAsync(async () =>
            {
                started1 = true;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                done1 = true;
            });
            var task2 = asyncSemaphore.ExecuteAsync(async () =>
            {
                started2 = true;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            });
            while (!started1) await Task.Delay(TimeSpan.FromMilliseconds(1));
            while (!task1.IsCompleted && !done1)
            {
                UT.Assert.IsFalse(started2);
                await Task.Delay(TimeSpan.FromMilliseconds(1));
            }

            await Task.WhenAll(task1, task2);
        }

        [TestMethod]
        public Task VerifyCancellationWorks()
        {
            var tokenSource = new CancellationTokenSource();
            var asyncSemaphore = new NexusAsyncSemaphore(TimeSpan.FromDays(1));
            var task = asyncSemaphore.ExecuteAsync((t) => Task.Delay(TimeSpan.FromDays(1), t), tokenSource.Token);
            tokenSource.Cancel();
            // 10ms grace period for task to finish 
            try
            {
                task.Wait(TimeSpan.FromMilliseconds(10));
                UT.Assert.IsTrue(task.IsCompleted, "The cancellation token did not affect the execution.");
            }
            catch (Exception e)
            {
                // Expects a TaskCanceledException
                if (!(e is TaskCanceledException) &&
                    !(e is AggregateException && e.InnerException is TaskCanceledException))
                {
                    UT.Assert.Fail($"Unexpected exception: {e}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
