using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Libraries.Core.Tests.Misc
{
    [TestClass]
    public class CircuitBreakerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CircuitBreakerTests));
        }

        [TestMethod]
        public async Task TODO()
        {
        }
    }
}