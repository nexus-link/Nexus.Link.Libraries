using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.Blob;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Test
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
            _storage = new CrudAzureStorageBlob<TestItemBare, TestItemId<Guid>, Guid>(connectionString, "test-container");
        }

        protected override ICrud<TestItemBare, TestItemId<Guid>, Guid> CrudStorage => _storage;

        [Ignore]
        public new async Task LockFailAsync()
        {
            await Task.Yield();
        }

        [Ignore]
        public new async Task LockReleaseLockAsync()
        {
            await Task.Yield();
        }

        [Ignore]
        public new async Task Lock_Async()
        {
            await Task.Yield();
        }
    }
}