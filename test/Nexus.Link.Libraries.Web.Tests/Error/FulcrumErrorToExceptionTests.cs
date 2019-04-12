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
        public void ConvertUnknownType()
        {
            var fulcrumError = new FulcrumError
            {
                Type = "UnknownErrorType"
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);
            Assert.IsNotNull(fulcrumException);
            Assert.AreEqual(FulcrumAssertionFailedException.ExceptionType, fulcrumException.Type);
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
        public void ConvertToOtherType()
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

        [TestMethod]
        public void ConvertedType()
        {
            var fulcrumError = new FulcrumError
            {
                Type = FulcrumAssertionFailedException.ExceptionType,
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                IsRetryMeaningful = true,
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString()
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);

            // Equal
            Assert.IsNotNull(fulcrumException);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.TechnicalMessage);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.Message);
            Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, fulcrumException.RecommendedWaitTimeInSeconds);

            // NOT equal
            Assert.AreNotEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            Assert.AreNotEqual(fulcrumError.CorrelationId, fulcrumException.CorrelationId);
            Assert.AreNotEqual(fulcrumError.Code, fulcrumException.Code);
            Assert.AreNotEqual(fulcrumError.IsRetryMeaningful, fulcrumException.IsRetryMeaningful);
            Assert.AreNotEqual(fulcrumError.FriendlyMessage, fulcrumException.FriendlyMessage);
            Assert.AreNotEqual(fulcrumError.Type, fulcrumException.Type);
            Assert.AreEqual(FulcrumResourceException.ExceptionType, fulcrumException.Type);
            Assert.AreNotEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            Assert.AreNotEqual(fulcrumError.MoreInfoUrl, fulcrumException.MoreInfoUrl);
            Assert.IsNull(fulcrumException.ErrorLocation);

            // Other tests
            Assert.IsNull(fulcrumException.InnerException);
            Assert.AreEqual(fulcrumError.InstanceId, fulcrumException.ParentInstanceId);

        }

        [TestMethod]
        public void SameType()
        {
            var fulcrumError = new FulcrumError
            {
                Type = FulcrumConflictException.ExceptionType,
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                IsRetryMeaningful = true,
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString()
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);

            // Equal
            Assert.IsNotNull(fulcrumException);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.TechnicalMessage);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.Message);
            Assert.AreEqual(fulcrumError.Code, fulcrumException.Code);
            Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, fulcrumException.RecommendedWaitTimeInSeconds);
            Assert.AreEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            Assert.AreEqual(fulcrumError.FriendlyMessage, fulcrumException.FriendlyMessage);
            Assert.AreEqual(fulcrumError.Type, fulcrumException.Type);
            Assert.AreEqual(fulcrumError.MoreInfoUrl, fulcrumException.MoreInfoUrl);

            // NOT equal
            Assert.AreNotEqual(fulcrumError.CorrelationId, fulcrumException.CorrelationId);
            Assert.AreNotEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            Assert.AreNotEqual(fulcrumError.IsRetryMeaningful, fulcrumException.IsRetryMeaningful);
            Assert.IsNull(fulcrumException.ErrorLocation);

            // Other tests
            Assert.IsNull(fulcrumException.InnerException);
            Assert.AreEqual(fulcrumError.InstanceId, fulcrumException.ParentInstanceId);
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
