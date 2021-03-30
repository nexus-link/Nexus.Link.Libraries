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
        public async Task Handles_Success()
        {
        }

        [TestMethod]
        public async Task Breaks_Circuit_After_Failure()
        {
        }

        [TestMethod]
        public async Task Allows_One_Contender_After_Failure()
        {
            // Test with two contenders, one should fail quick, the other goes through with it's action
        }

        [TestMethod]
        public async Task Breaks_Circuit_Fast_For_Many_Parallell()
        {
        }
    }
}