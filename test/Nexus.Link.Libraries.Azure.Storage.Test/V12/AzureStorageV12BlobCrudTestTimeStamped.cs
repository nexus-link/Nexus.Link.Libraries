using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.V12.Blob;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Test.V12
{
    [TestClass]
    public class AzureStorageV12BlobCrudTestTimeStamped : TestICrudTimeStamped<string>
    {
        private CrudAzureStorageContainer<TestItemBare, TestItemTimestamped<string>> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageV12BlobCrudTestEtag));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageContainer<TestItemBare, TestItemTimestamped<string>>(connectionString, $"test-container-{Guid.NewGuid()}");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _storage.DeleteAllAsync();
        }

        protected override ICrud<TestItemBare, TestItemTimestamped<string>, string> CrudStorage => _storage;
    }
}