using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.Test.NuGet.Crd;
using Nexus.Link.Libraries.Crud.Test.NuGet.Model;

namespace Nexus.Link.Libraries.Crud.Test.Core.Storage
{
    [TestClass]
    public class MemoryCrudTestParameters : TestParameters
    {

        /// <inheritdoc />
        public MemoryCrudTestParameters() : base(new CrudMemory<TestItemBare, Guid>())
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(MemoryCrudTestParameters));
        }
    }
}
