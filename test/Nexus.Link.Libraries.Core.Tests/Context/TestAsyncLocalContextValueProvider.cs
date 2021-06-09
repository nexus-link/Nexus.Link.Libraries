using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Context
{
    [TestClass]
    public class TestAsyncLocalContextValueProvider
    {
        private IContextValueProvider _provider;
        private readonly object _lockObject = new object();
        private int _failureCount;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TestAsyncLocalContextValueProvider));
            _provider = new AsyncLocalContextValueProvider();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _provider.Reset();
        }

        [TestMethod]
        public void NotInitializedValueIsNull()
        {
            UT.Assert.IsNull(_provider.GetValue<string>("X"));
        }

        [TestMethod]
        public void SetGet()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            UT.Assert.AreEqual(value1, _provider.GetValue<string>("X"));
        }

        [TestMethod]
        public void Reset()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            _provider.Reset();
            UT.Assert.IsNull(_provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task AsyncMethodCanRead()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            var value2 = await GetValueAsync<string>("X");
            UT.Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        public async Task AsyncMethodCanNotChangeToTop()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            var value2 = "Value2";
            await SetValueAsync<string>("X", value2);
            UT.Assert.AreEqual(value1, _provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task AsyncMethodCanNotCreateToTop()
        {
            var value = "Value1";
            await SetValueAsync<string>("X", value);
            UT.Assert.IsNull(_provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task ResetAndSetInAsyncMethodDoesNotChangeTop()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            var value2 = "Value2";
            await ResetAndSetAsync("X", value2);
            UT.Assert.AreEqual(value1, _provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task ResetAndSetInAsyncMethodDoesNotCreateTop()
        {
            var value1 = "Value1";
            await ResetAndSetAsync("X", value1);
            UT.Assert.IsNull(_provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task AsyncMethodDoesNotReadOutsideScope()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            var task = GetValueAsync<string>("X", TimeSpan.FromMilliseconds(100));
            var value2 = "Value2";
            _provider.SetValue("X", value2);
            UT.Assert.AreEqual(value1, await task);
        }

        [TestMethod]
        public async Task NewThreadCanRead()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            string value2 = null;
            var thread = new Thread(() => value2 = _provider.GetValue<string>("X"));
            thread.Start();
            while (thread.IsAlive) await Task.Delay(10);
            UT.Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        public async Task NewThreadCanNotChangeToTop()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            var value2 = "Value2";
            var thread = new Thread(() => _provider.SetValue("X", value2));
            thread.Start();
            while (thread.IsAlive) await Task.Delay(10);
            UT.Assert.AreEqual(value1, _provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task NewThreadCanNotCreateToTop()
        {
            var value1 = "Value1";
            var thread = new Thread(() => _provider.SetValue("X", value1));
            thread.Start();
            while (thread.IsAlive) await Task.Delay(10);
            UT.Assert.IsNull(_provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task ResetAndSetInNewThreadDoesNotChangeTop()
        {
            var value1 = "Value1";
            _provider.SetValue("X", value1);
            var value2 = "Value2";
            var thread = new Thread(() =>
            {
                _provider.Reset();
                _provider.SetValue("X", value2);
            });
            thread.Start();
            while (thread.IsAlive) await Task.Delay(10);
            UT.Assert.AreEqual(value1, _provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task ResetAndSetInNewThreadDoesNotCreateTop()
        {
            var value1 = "Value1";
            var thread = new Thread(() =>
            {
                _provider.Reset();
                _provider.SetValue("X", value1);
            });
            thread.Start();
            while (thread.IsAlive) await Task.Delay(10);
            UT.Assert.IsNull(_provider.GetValue<string>("X"));
        }

        [TestMethod]
        public async Task MassiveParallelAsyncCalls()
        {
            const int size = 100;

            var queue = new ConcurrentQueue<Message>();
            for (int i = 0; i < 100; i++)
            {
                _provider.Reset();
                var message = new Message
                {
                    ContextId = _provider.ContextId,
                    Context = _provider.SaveContext()
                };
                UT.Assert.AreNotEqual(default, message.ContextId);
                UT.Assert.IsNotNull(message.Context);
                queue.Enqueue(message);
            }
            UT.Assert.AreEqual(size, queue.Count);
            var taskList = new List<Task>();
            _failureCount = 0;
            while (queue.TryDequeue(out var message))
            {
                _provider.RestoreContext(message.Context);
                UT.Assert.AreEqual(message.ContextId, _provider.ContextId);
                var task = SlowAction(message, TimeSpan.FromMilliseconds(100));
                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
            UT.Assert.AreEqual(0, _failureCount);
        }
        private class Message
        {
            public Guid ContextId { get; set; }
            public IDictionary<string, object> Context { get; set; }

        }

        private async Task SlowAction(Message message, TimeSpan delay)
        {
            await Task.Delay(delay);
            if (message.ContextId != _provider.ContextId) Interlocked.Increment(ref _failureCount);
        } 

        private async Task<T> GetValueAsync<T>(string key)
        {
            return await GetValueAsync<T>(key, TimeSpan.FromMilliseconds(10));
        }

        private async Task<T> GetValueAsync<T>(string key, TimeSpan delay)
        {
            await Task.Delay(delay);
            return _provider.GetValue<T>(key);
        }

        private async Task SetValueAsync<T>(string key, T value)
        {
            await Task.Delay(10);
            _provider.SetValue(key, value);
        }

        private async Task ResetAndSetAsync<T>(string key, T value)
        {
            _provider.Reset();
            await SetValueAsync(key, value);
        }
    }
}
