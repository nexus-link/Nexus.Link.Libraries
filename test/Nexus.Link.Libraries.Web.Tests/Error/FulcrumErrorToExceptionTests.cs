using System;
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

namespace Nexus.Link.Libraries.Web.Tests.Error
{
    [TestClass]
    public class FulcrumErrorToExceptionTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(FulcrumErrorToExceptionTests).FullName);
        }

        [TestMethod]
        public void ConvertToSameType()
        {
            Verify(FulcrumBusinessRuleException.ExceptionType);
            Verify(FulcrumConflictException.ExceptionType);
            Verify(FulcrumNotFoundException.ExceptionType);
            Verify(FulcrumTryAgainException.ExceptionType);
        }

        [TestMethod]
        public void ConvertToOtherTypeType()
        {
            Verify(
                FulcrumAssertionFailedException.ExceptionType,
                FulcrumResourceException.ExceptionType);
            Verify(
                FulcrumContractException.ExceptionType,
                FulcrumResourceException.ExceptionType);
            Verify(
                FulcrumForbiddenAccessException.ExceptionType,
                FulcrumContractException.ExceptionType);
            Verify(
                FulcrumNotImplementedException.ExceptionType,
                FulcrumResourceException.ExceptionType);
            Verify(
                FulcrumServiceContractException.ExceptionType,
                FulcrumContractException.ExceptionType);
            Verify(
                FulcrumUnauthorizedException.ExceptionType,
                FulcrumContractException.ExceptionType);

        }

        private void Verify(string type)
        {
            Verify(type, type);
        }

        private static void Verify(string sourceType, string targetType)
        {
            var fulcrumError = new FulcrumError
            {
                Type = sourceType
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);
            Assert.IsNotNull(fulcrumException);
            Assert.AreEqual(targetType, fulcrumException.Type);
        }
    }
}
