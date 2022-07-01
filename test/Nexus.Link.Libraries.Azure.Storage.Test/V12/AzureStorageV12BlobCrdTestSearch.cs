using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.V12.Blob;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Test.V12
{
    [TestClass]
    public class AzureStorageV12BlobCrdTestSearch: TestICrdSearch<string>
    {
        private ICrud<TestItemSort<string>, TestItemSort<string>, string> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageV12BlobCrdTestSearch));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageContainer<TestItemSort<string>, TestItemSort<string>>(connectionString, $"test-container-{Guid.NewGuid()}");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _storage.DeleteAllAsync();
        }

        protected override ICrud<TestItemSort<string>, TestItemSort<string>, string> CrdStorage => _storage;

        protected override ICrud<TestItemSort<string>, TestItemSort<string>, string> CrudStorage => _storage;
    }
}
