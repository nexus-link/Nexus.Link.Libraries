using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.Table;
using Nexus.Link.Libraries.Azure.Storage.Test.Model;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Libraries.Azure.Storage.Test
{
    [TestClass]
    public class AzureStorageTableTest
    {
        private AzureStorageTable<Car, Car> _table;
        private const string PartitionKey = "carsV1";

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageQueueTest));
            var connectionString = TestSettings.ConnectionString;
            Assert.IsNotNull(connectionString);

            _table = new AzureStorageTable<Car, Car>(connectionString, $"{nameof(AzureStorageTableTest)}V1");
        }

        [TestMethod]
        public async Task DeleteAsync()
        {
            var item = new Car { Id = Guid.NewGuid().ToString(), Name = "Trabant", Etag = "1.0" };
            await _table.CreateAsync(PartitionKey, item.Id, item);

            var createdItem = await _table.ReadAsync(PartitionKey, item.Id);
            Assert.IsNotNull(createdItem?.Etag);
            Assert.AreNotEqual("1.0", createdItem.Etag);

            await _table.DeleteAsync(PartitionKey, createdItem.Id);
            var deletedItem = await _table.ReadAsync(PartitionKey, item.Id);
            Assert.IsNull(deletedItem);
        }
    }
}
