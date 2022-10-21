using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Web.Error.Logic;


// TODO: Consolidate Converted- and Same-Type exceptions to "one" test with datarows for all types

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
        [DataRow(FulcrumBusinessRuleException.ExceptionType)]
        [DataRow(FulcrumConflictException.ExceptionType)]
        [DataRow(FulcrumForbiddenAccessException.ExceptionType)]
        [DataRow(FulcrumResourceLockedException.ExceptionType)]
        [DataRow(FulcrumNotFoundException.ExceptionType)]
        [DataRow(FulcrumTryAgainException.ExceptionType)]
        [DataRow(FulcrumUnauthorizedException.ExceptionType)]
        [DataRow(FulcrumRedirectException.ExceptionType)]
        public void ConvertToSameType(string exceptionType)
        {
            Verify(exceptionType);
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
                FulcrumNotImplementedException.ExceptionType,
                FulcrumResourceException.ExceptionType);
            Verify(
                FulcrumServiceContractException.ExceptionType,
                FulcrumContractException.ExceptionType);

        }

        [TestMethod]
        [DataRow(FulcrumAssertionFailedException.ExceptionType)]
        public void ConvertedTypeNoInnerError(string exceptionType)
        {
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
            var fulcrumError = new FulcrumError
            {
                Type = exceptionType,
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

            Assert.AreNotEqual(fulcrumError.Type, fulcrumException.Type);

            // Equal
            Assert.IsNotNull(fulcrumException);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.TechnicalMessage);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.Message);
            Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, fulcrumException.RecommendedWaitTimeInSeconds);
            Assert.AreEqual(fulcrumError.IsRetryMeaningful, fulcrumException.IsRetryMeaningful);

            // NOT equal
            Assert.AreNotEqual(fulcrumError.CorrelationId, fulcrumException.CorrelationId);
            Assert.AreNotEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            Assert.AreNotEqual(fulcrumError.Code, fulcrumException.Code);
            Assert.AreNotEqual(fulcrumError.FriendlyMessage, fulcrumException.FriendlyMessage);
            Assert.AreEqual(FulcrumResourceException.ExceptionType, fulcrumException.Type);
            Assert.AreNotEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            Assert.AreNotEqual(fulcrumError.MoreInfoUrl, fulcrumException.MoreInfoUrl);
            Assert.IsNull(fulcrumException.ErrorLocation);

            // Inner exception
            Assert.IsNotNull(fulcrumException.InnerException);
            var innerFulcrumException = fulcrumException.InnerException as FulcrumException;
            Assert.IsNotNull(innerFulcrumException);
            Assert.AreEqual(fulcrumError.InstanceId, innerFulcrumException.InstanceId);
            Assert.AreEqual(fulcrumError.InnerInstanceId, innerFulcrumException.InnerInstanceId);
            Assert.AreEqual(fulcrumError.ServerTechnicalName, innerFulcrumException.ServerTechnicalName);
            Assert.AreEqual(fulcrumError.CorrelationId, innerFulcrumException.CorrelationId);
            Assert.AreEqual(fulcrumError.Code, innerFulcrumException.Code);
            Assert.AreEqual(fulcrumError.IsRetryMeaningful, innerFulcrumException.IsRetryMeaningful);
            Assert.AreEqual(fulcrumError.FriendlyMessage, innerFulcrumException.FriendlyMessage);
            Assert.AreEqual(fulcrumError.Type, innerFulcrumException.Type);
            Assert.AreEqual(fulcrumError.InstanceId, innerFulcrumException.InstanceId);
            Assert.AreEqual(fulcrumError.MoreInfoUrl, innerFulcrumException.MoreInfoUrl);
            Assert.AreEqual(fulcrumError.ErrorLocation, innerFulcrumException.ErrorLocation);
        }

        [TestMethod]
        [DataRow(FulcrumAssertionFailedException.ExceptionType)]
        public void ConvertedTypeInnerErrorOfFulcrumType(string exceptionType)
        {
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
            var innerFulcrumError = new FulcrumError
            {
                TechnicalMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                Type = FulcrumContractException.ExceptionType
            };
            var fulcrumError = new FulcrumError
            {
                Type = exceptionType,
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                IsRetryMeaningful = true,
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString(),
                InnerError = innerFulcrumError,
                InnerInstanceId = innerFulcrumError.InstanceId
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);

            // Inner exception
            Assert.IsNotNull(fulcrumException.InnerException);
            var innerFulcrumException = fulcrumException.InnerException as FulcrumException;
            Assert.IsNotNull(innerFulcrumException);
            Assert.AreEqual(fulcrumError.InstanceId, innerFulcrumException.InstanceId);
            Assert.AreEqual(fulcrumError.InnerInstanceId, innerFulcrumException.InnerInstanceId);

            // Inner inner exception
            Assert.IsNotNull(innerFulcrumException.InnerException);
            var innerInnerFulcrumException = innerFulcrumException.InnerException as FulcrumException;
            Assert.IsNotNull(innerInnerFulcrumException);
            Assert.AreEqual(fulcrumError.InnerInstanceId, innerInnerFulcrumException.InstanceId);
        }

        [TestMethod]
        [DataRow(FulcrumAssertionFailedException.ExceptionType)]
        public void ConvertedTypeInnerErrorOfNonFulcrumType(string exceptionType)
        {
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
            var innerFulcrumError = new FulcrumError
            {
                TechnicalMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                Type = "NotFulcrum"
            };
            var fulcrumError = new FulcrumError
            {
                Type = exceptionType,
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                IsRetryMeaningful = true,
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString(),
                InnerError = innerFulcrumError,
                InnerInstanceId = innerFulcrumError.InstanceId
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);

            // Inner exception
            Assert.IsNotNull(fulcrumException.InnerException);
            var innerFulcrumException = fulcrumException.InnerException as FulcrumException;
            Assert.IsNotNull(innerFulcrumException);
            Assert.AreEqual(fulcrumError.InstanceId, innerFulcrumException.InstanceId);

            // Inner inner exception
            Assert.IsNull(innerFulcrumException.InnerException);
            Assert.AreEqual(fulcrumError.InnerInstanceId, innerFulcrumException.InnerInstanceId);
        }

        [TestMethod]
        [DataRow(FulcrumBusinessRuleException.ExceptionType)]
        [DataRow(FulcrumConflictException.ExceptionType)]
        [DataRow(FulcrumForbiddenAccessException.ExceptionType)]
        [DataRow(FulcrumResourceLockedException.ExceptionType)]
        [DataRow(FulcrumNotFoundException.ExceptionType)]
        [DataRow(FulcrumTryAgainException.ExceptionType)]
        [DataRow(FulcrumUnauthorizedException.ExceptionType)]
        public void SameTypeNoInnerError(string exceptionType)
        {
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
            var fulcrumError = new FulcrumError
            {
                Type = exceptionType,
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

            Assert.AreEqual(fulcrumError.Type, fulcrumException.Type);

            // Equal
            Assert.IsNotNull(fulcrumException);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.TechnicalMessage);
            Assert.AreEqual(fulcrumError.TechnicalMessage, fulcrumException.Message);
            Assert.AreEqual(fulcrumError.Code, fulcrumException.Code);
            Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, fulcrumException.RecommendedWaitTimeInSeconds);
            Assert.AreEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            Assert.AreEqual(fulcrumError.FriendlyMessage, fulcrumException.FriendlyMessage);
            Assert.AreEqual(fulcrumError.MoreInfoUrl, fulcrumException.MoreInfoUrl);
            Assert.AreEqual(fulcrumError.CorrelationId, fulcrumException.CorrelationId);
            Assert.AreEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            Assert.AreEqual(fulcrumError.IsRetryMeaningful, fulcrumException.IsRetryMeaningful);
            Assert.AreEqual(fulcrumError.ErrorLocation, fulcrumException.ErrorLocation);
            Assert.AreEqual(fulcrumError.InnerInstanceId, fulcrumException.InnerInstanceId);

            // Other tests
            Assert.IsNull(fulcrumException.InnerException);
        }

        [TestMethod]
        [DataRow(FulcrumBusinessRuleException.ExceptionType)]
        [DataRow(FulcrumConflictException.ExceptionType)]
        [DataRow(FulcrumForbiddenAccessException.ExceptionType)]
        [DataRow(FulcrumResourceLockedException.ExceptionType)]
        [DataRow(FulcrumNotFoundException.ExceptionType)]
        [DataRow(FulcrumTryAgainException.ExceptionType)]
        [DataRow(FulcrumUnauthorizedException.ExceptionType)]
        public void SameTypeInnerErrorOfFulcrumType(string exceptionType)
        {
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
            var innerFulcrumError = new FulcrumError
            {
                TechnicalMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                Type = FulcrumContractException.ExceptionType
            };
            var fulcrumError = new FulcrumError
            {
                Type = exceptionType,
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                IsRetryMeaningful = true,
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString(),
                InnerError = innerFulcrumError,
                InnerInstanceId = innerFulcrumError.InstanceId
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);

            // Inner exception
            Assert.IsNotNull(fulcrumException.InnerException);
            var innerFulcrumException = fulcrumException.InnerException as FulcrumException;
            Assert.IsNotNull(innerFulcrumException);
            Assert.AreEqual(fulcrumError.InnerInstanceId, innerFulcrumException.InstanceId);
        }

        [TestMethod]
        [DataRow(FulcrumBusinessRuleException.ExceptionType)]
        [DataRow(FulcrumConflictException.ExceptionType)]
        [DataRow(FulcrumForbiddenAccessException.ExceptionType)]
        [DataRow(FulcrumResourceLockedException.ExceptionType)]
        [DataRow(FulcrumNotFoundException.ExceptionType)]
        [DataRow(FulcrumTryAgainException.ExceptionType)]
        [DataRow(FulcrumUnauthorizedException.ExceptionType)]
        public void SameTypeInnerErrorOfNonFulcrumType(string exceptionType)
        {
            FulcrumApplication.Context.CorrelationId = Guid.NewGuid().ToString();
            var innerFulcrumError = new FulcrumError
            {
                TechnicalMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                Type = "NotFulcrum"
            };
            var fulcrumError = new FulcrumError
            {
                Type = exceptionType,
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                InstanceId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                IsRetryMeaningful = true,
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString(),
                InnerError = innerFulcrumError,
                InnerInstanceId = innerFulcrumError.InstanceId
            };
            var fulcrumException = ExceptionConverter.ToFulcrumException(fulcrumError);

            // Inner exception
            Assert.IsNull(fulcrumException.InnerException);
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
            if (fulcrumException is FulcrumRedirectException redirectException)
            {
                redirectException.FromId = Guid.NewGuid().ToString();
                redirectException.ToId = Guid.NewGuid().ToString();
            }
        }
    }
}
