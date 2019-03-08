using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Nexus.Link.Libraries.Core.Tests.Error
{
    [TestClass]
    public class ErrorTest
    {
        [TestMethod]
        public void TestStackTrace()
        {
            var x = new FulcrumUnauthorizedException("x");
            // Just access StackTrace gave NRE before
            UT.Assert.IsNotNull(x.StackTrace);
        }
    }
}
