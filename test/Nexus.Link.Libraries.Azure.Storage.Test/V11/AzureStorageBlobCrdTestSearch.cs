﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Azure.Storage.Blob;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.UnitTests.Crd;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
#pragma warning disable CS0618

namespace Nexus.Link.Libraries.Azure.Storage.Test.V11
{
    [TestClass]
    public class AzureStorageBlobCrdTestSearch: TestICrdSearch<Guid>
    {
        private ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> _storage;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(AzureStorageBlobCrdTestSearch));
            var connectionString = TestSettings.ConnectionString;
            _storage = new CrudAzureStorageBlob<TestItemSort<Guid>, TestItemSort<Guid>, Guid>(connectionString, $"test-container-{Guid.NewGuid()}");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await _storage.DeleteAllAsync();
        }

        protected override ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> CrdStorage => _storage;

        protected override ICrud<TestItemSort<Guid>, TestItemSort<Guid>, Guid> CrudStorage => _storage;
    }
}
