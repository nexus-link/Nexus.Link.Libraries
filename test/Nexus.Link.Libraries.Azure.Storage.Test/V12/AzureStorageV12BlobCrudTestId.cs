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
    public class AzureStorageV12BlobCrudTestId : TestICrudId<string>
    {
        private CrudAzureStorageContainer<TestItemBare, TestItemId<string>> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageV12BlobCrudTestId));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageContainer<TestItemBare, TestItemId<string>>(connectionString, $"test-container-{Guid.NewGuid()}");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _storage.DeleteAllAsync();
        }

        protected override ICrud<TestItemBare, TestItemId<string>, string> CrudStorage => _storage;

        [Ignore]
        public new Task ClaimLock()
        {
            return Task.CompletedTask;
        }

        [Ignore]
        public new Task ClaimLock_Given_AlreadyLocked_GivesException()
        {
            return Task.CompletedTask;
        }

        [Ignore]
        public new Task Claim_Release_Claim()
        {
            return Task.CompletedTask;
        }

        [Ignore]
        public new Task Claim_Reclaim()
        {
            return Task.CompletedTask;
        }
    }
}