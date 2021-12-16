using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage.Queue;
using Nexus.Link.Libraries.Azure.Storage.Queue;
using Nexus.Link.Libraries.Azure.Storage.Test.V11.Model;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Azure.Storage.Test.V11
{
    [TestClass]
    public class AzureStorageQueueTest
    {
        private AzureStorageQueue<Message> _queue;

        [TestInitialize]
        public async Task InitializeAsync()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageQueueTest));
            var connectionString = TestSettings.ConnectionString;
            Assert.IsNotNull(connectionString);
            _queue = new AzureStorageQueue<Message>(connectionString, "test-queue");
            await _queue.ClearAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumResourceException))]
        public async Task InitializeNonExistingAsync()
        {
            var connectionString = TestSettings.ConnectionStringNonExisting;
            Assert.IsNotNull(connectionString);
            var queue = new AzureStorageQueue<Message>(connectionString, "test-queue", new QueueRequestOptions()
            {
                MaximumExecutionTime = TimeSpan.FromMilliseconds(500)
            });
            await queue.ClearAsync();
        }

        [TestMethod]
        public async Task GetDoesNotBlockAsync()
        {
            var stopwatch = new Stopwatch();
            var getTask = _queue.GetOneMessageNoBlockAsync();
            stopwatch.Start();
            while (!getTask.IsCompleted)
            {
                Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(1), "Expected the method to finish quickly.");
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
            Assert.IsNull(await getTask);
        }

        [TestMethod]
        public async Task PeekDoesNotBlockAsync()
        {
            var stopwatch = new Stopwatch();
            var getTask = _queue.PeekNoBlockAsync();
            stopwatch.Start();
            while (!getTask.IsCompleted)
            {
                Assert.IsTrue(stopwatch.Elapsed < TimeSpan.FromSeconds(1), "Expected the method to finish quickly.");
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
            Assert.IsTrue(getTask.IsCompleted, "Expected the method to finish quickly.");
            Assert.IsNull(await getTask);
        }

        [TestMethod]
        public async Task MessageGetsThroughAsync()
        {
            var message = new Message { Name = "Message1" };
            await _queue.AddMessageAsync(message);
            var result = await _queue.GetOneMessageNoBlockAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(message.Name, result.Name);
        }

        [TestMethod]
        public async Task MessageCountIsIncreasedWhenAddingAsync()
        {
            var expectedCount = 5;

            for (int i = 0; i < expectedCount; i++)
            {
                var message = new Message { Name = "Message1" };
                await _queue.AddMessageAsync(message);
            }

            var result = await _queue.GetApproximateMessageCountAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCount, result);
        }

        [TestMethod]
        public async Task PeekAndGetAsync()
        {
            var message = new Message { Name = "Message1" };
            await _queue.AddMessageAsync(message);
            var result = await _queue.PeekNoBlockAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(message.Name, result.Name);
            result = await _queue.GetOneMessageNoBlockAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(message.Name, result.Name);
        }

        [TestMethod]
        public async Task ClearQueueAsync()
        {
            var message = new Message { Name = "Message1" };
            await _queue.AddMessageAsync(message);
            await _queue.ClearAsync();
            var getTask = _queue.GetOneMessageNoBlockAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            Assert.IsTrue(getTask.IsCompleted, "Expected the method to finish quickly.");
            Assert.IsNull(await getTask);
        }

        [TestMethod]
        public async Task DeleteMessageAsync()
        {
            var message = new Message { Name = "Message1" };
            await _queue.AddMessageAsync(message);
            var result = await _queue.PeekNoBlockAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(message.Name, result.Name);
            result = await _queue.GetOneMessageNoBlockAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(message.Name, result.Name);

            result = await _queue.GetOneMessageNoBlockAsync();
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task HealthAsync()
        {
            var result = await _queue.GetResourceHealthAsync();
        }
    }
}
