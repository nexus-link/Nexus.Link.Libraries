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
    public class AzureStorageV12BlobCrudTestValidated : TestICrudValidated<string>
    {
        private CrudAzureStorageContainer<TestItemBare, TestItemValidated<string>> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageV12BlobCrudTestValidated));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageContainer<TestItemBare, TestItemValidated<string>>(connectionString, $"test-container-{Guid.NewGuid()}");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _storage.DeleteAllAsync();
        }

        protected override ICrud<TestItemBare, TestItemValidated<string>, string> CrudStorage => _storage;
    }
}