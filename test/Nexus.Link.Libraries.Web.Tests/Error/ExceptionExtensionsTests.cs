using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
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
            await AlwaysThrowAsync(new FulcrumTryAgainException("m"))
                .ShouldThrowAsync < FulcrumTryAgainException>();
            //await (AlwaysThrowAsync(new RequestAcceptedException("a"))
            //    .FireAndForgetAsync());
        }

        [TestMethod]
        public async Task FireAndForget_02_Given_OtherException_RethrowsException()
        {
            await AlwaysThrowAsync(new FulcrumTryAgainException("a"))
                .FireAndForgetAsync()
                .ShouldThrowAsync<FulcrumTryAgainException>();
        }

        [TestMethod]
        public async Task FireAndForget_03_Given_OtherExceptionAndIgnoreAll_SwallowsException()
        {
            await AlwaysThrowAsync(new FulcrumTryAgainException("a"))
                .FireAndForgetAsync(true);
        }

        [TestMethod]
        public async Task FireAndForget2_01_Given_RequestAcceptedException_IgnoresException()
        {
            await AlwaysThrowAsync(1, new RequestAcceptedException("a"))
                .FireAndForgetAsync();
        }

        [TestMethod]
        public async Task FireAndForget2_02_Given_OtherException_RethrowsException()
        {
            await AlwaysThrowAsync(1, new FulcrumTryAgainException("a"))
                .FireAndForgetAsync()
                .ShouldThrowAsync<FulcrumTryAgainException>();
        }

        [TestMethod]
        public async Task FireAndForget2_03_Given_OtherExceptionAndIgnoreAll_SwallowsException()
        {
            await AlwaysThrowAsync(1, new FulcrumTryAgainException("a"))
                .FireAndForgetAsync(true);
        }

        private Task<int> AlwaysThrowAsync(int i, Exception e)
        {
            return Task.FromException<int>(e);
        }

        private Task AlwaysThrowAsync(Exception e)
        {
            return Task.FromException(e);
        }
    }
}
