using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
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

        [TestMethod]
        public void ErrorDifferentType()
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
            var fulcrumException = new FulcrumResourceException(Guid.NewGuid().ToString());
            fulcrumException.CopyFrom(fulcrumError);

            // Equal
            UT.Assert.AreEqual(fulcrumException.TechnicalMessage, fulcrumException.Message);
            UT.Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, fulcrumException.RecommendedWaitTimeInSeconds);

            // NOT equal
            UT.Assert.AreNotEqual(fulcrumError.TechnicalMessage, fulcrumException.TechnicalMessage);
            UT.Assert.AreNotEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            UT.Assert.AreNotEqual(fulcrumError.CorrelationId, fulcrumException.CorrelationId);
            UT.Assert.AreNotEqual(fulcrumError.Code, fulcrumException.Code);
            UT.Assert.AreNotEqual(fulcrumError.IsRetryMeaningful, fulcrumException.IsRetryMeaningful);
            UT.Assert.AreNotEqual(fulcrumError.FriendlyMessage, fulcrumException.FriendlyMessage);
            UT.Assert.AreNotEqual(fulcrumError.Type, fulcrumException.Type);
            UT.Assert.AreEqual(FulcrumResourceException.ExceptionType, fulcrumException.Type);
            UT.Assert.AreNotEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            UT.Assert.AreNotEqual(fulcrumError.MoreInfoUrl, fulcrumException.MoreInfoUrl);
            UT.Assert.IsNull(fulcrumException.ErrorLocation);

            // Other tests
            UT.Assert.IsNull(fulcrumException.InnerException);
            UT.Assert.AreEqual(fulcrumError.InstanceId, fulcrumException.ParentInstanceId);

        }

        [TestMethod]
        public void ErrorSameType()
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
            var fulcrumException = new FulcrumConflictException(Guid.NewGuid().ToString());
            fulcrumException.CopyFrom(fulcrumError);

            // Equal
            UT.Assert.AreEqual(fulcrumError.Code, fulcrumException.Code);
            UT.Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, fulcrumException.RecommendedWaitTimeInSeconds);
            UT.Assert.AreEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            UT.Assert.AreEqual(fulcrumError.FriendlyMessage, fulcrumException.FriendlyMessage);
            UT.Assert.AreEqual(fulcrumError.Type, fulcrumException.Type);
            UT.Assert.AreEqual(fulcrumError.MoreInfoUrl, fulcrumException.MoreInfoUrl);
            UT.Assert.AreEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);

            // NOT equal
            UT.Assert.AreNotEqual(fulcrumError.CorrelationId, fulcrumException.CorrelationId);
            UT.Assert.AreNotEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            UT.Assert.AreNotEqual(fulcrumError.IsRetryMeaningful, fulcrumException.IsRetryMeaningful);
            UT.Assert.IsNull(fulcrumException.ErrorLocation);

            // Other tests
            UT.Assert.IsNull(fulcrumException.InnerException);
            UT.Assert.AreEqual(fulcrumError.InstanceId, fulcrumException.ParentInstanceId);
            UT.Assert.AreEqual(fulcrumException.TechnicalMessage, fulcrumException.Message);
        }

        [TestMethod]
        public void ExceptionToError()
        {
            var fulcrumException = new FulcrumAssertionFailedException(Guid.NewGuid().ToString())
            {
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString()
            };
            var fulcrumError = new FulcrumError();
            fulcrumError.CopyFrom(fulcrumException);

            // Equal
            UT.Assert.AreEqual(fulcrumError.Code, fulcrumException.Code);
            UT.Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, fulcrumException.RecommendedWaitTimeInSeconds);
            UT.Assert.AreEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            UT.Assert.AreEqual(fulcrumError.FriendlyMessage, fulcrumException.FriendlyMessage);
            UT.Assert.AreEqual(fulcrumError.Type, fulcrumException.Type);
            UT.Assert.AreEqual(fulcrumError.MoreInfoUrl, fulcrumException.MoreInfoUrl);
            UT.Assert.AreEqual(fulcrumError.ServerTechnicalName, fulcrumException.ServerTechnicalName);
            UT.Assert.AreEqual(fulcrumError.CorrelationId, fulcrumException.CorrelationId);
            UT.Assert.AreEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            UT.Assert.AreEqual(fulcrumError.IsRetryMeaningful, fulcrumException.IsRetryMeaningful);
            UT.Assert.AreEqual(fulcrumError.ErrorLocation, fulcrumException.ErrorLocation);
            UT.Assert.AreEqual(fulcrumError.InstanceId, fulcrumException.InstanceId);
            UT.Assert.AreEqual(fulcrumError.ParentInstanceId, fulcrumException.ParentInstanceId);

            // Other tests
            UT.Assert.IsNull(fulcrumException.InnerException);
        }

        [TestMethod]
        public void ExceptionToErrorAndBack()
        {
            var fulcrumException = new FulcrumConflictException(Guid.NewGuid().ToString())
            {
                TechnicalMessage = Guid.NewGuid().ToString(),
                FriendlyMessage = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                ErrorLocation = Guid.NewGuid().ToString(),
                MoreInfoUrl = Guid.NewGuid().ToString(),
                RecommendedWaitTimeInSeconds = 100.0,
                ServerTechnicalName = Guid.NewGuid().ToString()
            };
            var fulcrumError = new FulcrumError();
            fulcrumError.CopyFrom(fulcrumException);
            var copy = new FulcrumConflictException(fulcrumError.TechnicalMessage);
            copy.CopyFrom(fulcrumError);

            // Equal
            UT.Assert.AreEqual(fulcrumError.Code, copy.Code);
            UT.Assert.AreEqual(fulcrumError.RecommendedWaitTimeInSeconds, copy.RecommendedWaitTimeInSeconds);
            UT.Assert.AreEqual(fulcrumError.ServerTechnicalName, copy.ServerTechnicalName);
            UT.Assert.AreEqual(fulcrumError.FriendlyMessage, copy.FriendlyMessage);
            UT.Assert.AreEqual(fulcrumError.Type, copy.Type);
            UT.Assert.AreEqual(fulcrumError.MoreInfoUrl, copy.MoreInfoUrl);
            UT.Assert.AreEqual(fulcrumError.ServerTechnicalName, copy.ServerTechnicalName);
            UT.Assert.AreEqual(fulcrumError.InstanceId, copy.ParentInstanceId);
            UT.Assert.AreEqual(fulcrumError.IsRetryMeaningful, copy.IsRetryMeaningful);

            // Other tests
            UT.Assert.IsNull(copy.InnerException);
            UT.Assert.AreEqual(FulcrumApplication.Context.CorrelationId, copy.CorrelationId);
            UT.Assert.IsNull(copy.ErrorLocation);
        }
    }
}
