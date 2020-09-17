using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Threads;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Threads
{
    [TestClass]
    public class TestBasicThreadHandler
    {
        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestThreadHelper).FullName);
            FulcrumApplication.Setup.ThreadHandler = new BasicThreadHandler();
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
        }

        [TestMethod]
        public void ThreadsCanAccessKnownContextValue()
        {
            var correlationId = Guid.NewGuid().ToString();
            FulcrumApplication.Context.CorrelationId = correlationId;
            var done = false;
            var canAccess = false;
            var thread = new BasicThreadHandler().FireAndForget(token =>
            {
                canAccess = correlationId == FulcrumApplication.Context.CorrelationId;
                done = true;
            });
            UT.Assert.IsNotNull(thread);
            while (!done)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            UT.Assert.IsTrue(canAccess);
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
            UT.Assert.IsFalse(thread.IsAlive);
        }

        [TestMethod]
        public void ThreadsCanAccessCustomContextValue()
        {
            var customId = Guid.NewGuid();
            var provider1 = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");
            provider1.SetValue(customId);
            var done = false;
            var canAccess = false;
            var thread = new BasicThreadHandler().FireAndForget(token =>
            {
                var provider2 = new OneValueProvider<Guid>(FulcrumApplication.Context.ValueProvider, "Custom");
                canAccess = customId == provider2.GetValue();
                done = true;
            });
            UT.Assert.IsNotNull(thread);
            while (!done)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            UT.Assert.IsTrue(canAccess);
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
            UT.Assert.IsFalse(thread.IsAlive);
        }

        [TestMethod]
        public void IsAliveChanges()
        {
            var done = false;
            var thread = new BasicThreadHandler().FireAndForget(token =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
                done = true;
            });
            UT.Assert.IsNotNull(thread);
            UT.Assert.IsTrue(thread.IsAlive);
            while (!done)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(10));
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(10));
            UT.Assert.IsFalse(thread.IsAlive);
        }
    }
}
