using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.Blob;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.Test.NuGet.Crud;
using Nexus.Link.Libraries.Crud.Test.NuGet.Model;

namespace Nexus.Link.Libraries.Azure.Storage.Test
{
    [TestClass]
    public class AzureStorageBlobCrudTestEtag : TestICrudEtag<Guid>
    {
        private CrudAzureStorageBlob<TestItemBare, TestItemEtag<Guid>, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageBlobCrudTestEtag));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageBlob<TestItemBare, TestItemEtag<Guid>, Guid>(connectionString, "test-container");
        }

        protected override ICrud<TestItemBare, TestItemEtag<Guid>, Guid> CrudStorage => _storage;
    }
}