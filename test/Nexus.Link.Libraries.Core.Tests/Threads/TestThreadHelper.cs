using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Threads;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Threads
{
    [TestClass]
    public class TestThreadHelper
    {
        private bool _done;
        private Guid _foundValue;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestThreadHelper).FullName);
            FulcrumApplication.Setup.ThreadHandler = new BasicThreadHandler();
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
        }

        [TestMethod]
        public void FireAndForgetCanReadCustomContextValue()
        {
            Guid customId = Guid.Empty;
            Thread thread = null;
            var provider = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");

            // Action
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForget(() => FindCustomValue());
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, _foundValue);

            // Func<Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForget(async () => await FindCustomValueAsync());
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, _foundValue);

            // Func<CancellationToken, Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForget(async (ct) => await FindCustomValueAsync(), CancellationToken.None);
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, _foundValue);
        }

        [TestMethod]
        public void FireAndForgetResetContextCanNotReadCustomContextValue()
        {
            Guid customId = Guid.Empty;
            Thread thread = null;
            var provider = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");

            // Action
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetResetContext(() => FindCustomValue());
            WaitForThreadToComplete(thread);
            UT.Assert.IsTrue(_foundValue == Guid.Empty);

            // Func<Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetResetContext(async () => await FindCustomValueAsync());
            WaitForThreadToComplete(thread);
            UT.Assert.IsTrue(_foundValue == Guid.Empty);

            // Func<CancellationToken, Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetResetContext(async (ct) => await FindCustomValueAsync(), CancellationToken.None);
            WaitForThreadToComplete(thread);
            UT.Assert.IsTrue(_foundValue == Guid.Empty);
        }

        [TestMethod]
        public void FireAndForgetResetContextDoesNotNotInterfereWIthTop()
        {
            Guid customId = Guid.Empty;
            Thread thread = null;
            var provider = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");

            // Action
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetResetContext(() => FindCustomValue());
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, provider.GetValue());

            // Func<Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetResetContext(async () => await FindCustomValueAsync());
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, provider.GetValue());

            // Func<CancellationToken, Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetResetContext(async (ct) => await FindCustomValueAsync(),
                CancellationToken.None);
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, provider.GetValue());
        }

        [TestMethod]
        public void FireAndForgetWithExpensiveStackTracePreservationCanReadCustomContextValue()
        {
            Guid customId = Guid.Empty;
            Thread thread = null;
            var provider = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");

            // Action
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetWithExpensiveStackTracePreservation(() => FindCustomValue());
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, provider.GetValue());

            // Func<Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetWithExpensiveStackTracePreservation(async () => await FindCustomValueAsync());
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, provider.GetValue());

            // Func<CancellationToken, Task>
            customId = Guid.NewGuid();
            provider.SetValue(customId);
            _foundValue = Guid.NewGuid();
            thread = ThreadHelper.FireAndForgetWithExpensiveStackTracePreservation(async (ct) => await FindCustomValueAsync(),
                CancellationToken.None);
            WaitForThreadToComplete(thread);
            UT.Assert.AreEqual(customId, provider.GetValue());
        }

        [TestMethod]
        public void TestMaxDepth()
        {
            var correlationId = Guid.NewGuid().ToString();
            FulcrumApplication.Context.CorrelationId = correlationId;

            const int tries = 15;
            var count = 0;
            var actionCalled = new ManualResetEvent(false);
            for (var i = 0; i <= tries; i++)
            {
                ThreadHelper.FireAndForgetWithExpensiveStackTracePreservation(() =>
                {
                    lock (correlationId)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(100));
                        var canAccess = correlationId == FulcrumApplication.Context.CorrelationId;
                        UT.Assert.IsTrue(canAccess);
                        Console.WriteLine($"count: {count}");
                        if (++count == tries) actionCalled.Set();
                    }
                });
                Console.WriteLine($"i: {i}");
            }

            UT.Assert.IsTrue(actionCalled.WaitOne(TimeSpan.FromSeconds(3)), $"Could not finish the {tries} tasks");

        }

        [TestMethod]
        public async Task ThreadWaitsForAsyncJob()
        {
            _done = false;
            var thread = ThreadHelper.FireAndForgetResetContext(async () => await SleepAsync(TimeSpan.FromMilliseconds(100)));
            while (thread.IsAlive) await Task.Delay(TimeSpan.FromMilliseconds(1));
            UT.Assert.IsTrue(_done);
        }

        private async Task SleepAsync(TimeSpan delay)
        {
            await Task.Delay(delay);
            _done = true;
        }

        private void FindCustomValue()
        {
            var provider = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");
            _foundValue = provider.GetValue();
        }

        private async Task FindCustomValueAsync()
        {
            var provider = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");
            _foundValue = provider.GetValue();
            await Task.CompletedTask;
        }

        private static void WaitForThreadToComplete(Thread thread)
        {
            UT.Assert.IsNotNull(thread);
            while (thread.IsAlive)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }
        }
    }
}
