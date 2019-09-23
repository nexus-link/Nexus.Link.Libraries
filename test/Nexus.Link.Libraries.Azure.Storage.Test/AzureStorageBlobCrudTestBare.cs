using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.Blob;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crud;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Test
{
    [TestClass]
    public class AzureStorageBlobCrudTestBare : TestICrudBare<Guid>
    {
        private CrudAzureStorageBlob<TestItemBare, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageBlobCrudTestBare));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageBlob<TestItemBare, Guid>(connectionString, "test-container");
        }

        protected override ICrud<TestItemBare, TestItemBare, Guid> CrudStorage => _storage;
    }
}