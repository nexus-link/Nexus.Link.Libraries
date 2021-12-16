using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.Blob;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Azure.Storage.Test.V11
{
    [TestClass]
    public class AzureStorageBlobCrudTestId : TestICrudId<Guid>
    {
        private CrudAzureStorageBlob<TestItemBare, TestItemId<Guid>, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageBlobCrudTestId));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageBlob<TestItemBare, TestItemId<Guid>, Guid>(connectionString, $"test-container-{Guid.NewGuid()}");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _storage.DeleteAllAsync();
        }

        protected override ICrud<TestItemBare, TestItemId<Guid>, Guid> CrudStorage => _storage;

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