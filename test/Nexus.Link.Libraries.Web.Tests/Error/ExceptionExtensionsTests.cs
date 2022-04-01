using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Web.Error.Logic;
using Shouldly;

namespace Nexus.Link.Libraries.Web.Tests.Error
{
    [TestClass]
    public class ExceptionExtensionsTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ExceptionConverterTests).FullName);
        }

        [TestMethod]
        public async Task FireAndForget_01_Given_RequestAcceptedException_IgnoresException()
        {
            await Task.FromException(new RequestAcceptedException("a"))
                .FireAndForgetAsync();
        }

        [TestMethod]
        public async Task FireAndForget_01_Given_OtherException_RethrowsException()
        {
            await Task.FromException(new FulcrumTryAgainException("a"))
                .FireAndForgetAsync()
                .ShouldThrowAsync<FulcrumTryAgainException>();
        }

        [TestMethod]
        public async Task FireAndForget_01_Given_OtherExceptionAndIgnoreAll_SwallowsException()
        {
            await Task.FromException(new FulcrumTryAgainException("a"))
                .FireAndForgetAsync(true);
        }
    }
}
