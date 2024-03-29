﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Web.Error.Logic;

//TODO: ExceptionToFulcrumResponse

namespace Nexus.Link.Libraries.Web.Tests.Error
{
    [TestClass]
    public class ExceptionToFulcrumErrorTests
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ExceptionToFulcrumErrorTests).FullName);
        }

        [TestMethod]
        public void NullException()
        {
            var error = ExceptionConverter.ToFulcrumError(null);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void FulcrumException()
        {
            var exceptionMessage = Guid.NewGuid().ToString();
            var exception = new FulcrumAssertionFailedException(exceptionMessage);
            try
            {
                throw exception;
            }
            catch (FulcrumException e)
            {
                Assert.IsNotNull(e.StackTrace);
                var error = ExceptionConverter.ToFulcrumError(exception);
                Assert.IsNotNull(error);
                Assert.AreEqual(exceptionMessage, error.TechnicalMessage);
                Assert.IsNull(error.ErrorLocation,
                    $"Error location was expected to be null, but contained the following: {error.ErrorLocation}");
            }
        }

        [TestMethod]
        public void FulcrumExceptionWithInnerNonFulcrumException()
        {
            var innerMessage = Guid.NewGuid().ToString();
            var exceptionMessage = Guid.NewGuid().ToString();
            var innerException = new Exception(innerMessage);
            var exception = new FulcrumAssertionFailedException(exceptionMessage, innerException);
            try
            {
                throw exception;
            }
            catch (FulcrumException e)
            {
                Assert.IsNotNull(e.StackTrace);
                var error = ExceptionConverter.ToFulcrumError(exception);
                Assert.IsNotNull(error);
                Assert.AreEqual(exceptionMessage, error.TechnicalMessage);
                Assert.IsNull(error.ErrorLocation,
                    $"Error location was expected to be null, but contained the following: {error.ErrorLocation}");
                Assert.IsNotNull(error.InnerError);
                Assert.AreEqual(innerMessage, error.InnerError.TechnicalMessage);
                Assert.AreEqual(error.InnerInstanceId, error.InnerError?.InstanceId);
            }
        }

        [TestMethod]
        public void FulcrumException_ResourceLocked()
        {
            var exceptionMessage = Guid.NewGuid().ToString();
            var exception = new FulcrumResourceLockedException(exceptionMessage);
            try
            {
                throw exception;
            }
            catch (FulcrumException e)
            {
                Assert.IsNotNull(e.StackTrace);
                var error = ExceptionConverter.ToFulcrumError(exception);
                Assert.IsNotNull(error);
                Assert.AreEqual(exceptionMessage, error.TechnicalMessage);
                Assert.AreEqual("Xlent.Fulcrum.ResourceLocked", error.Type);

                Assert.IsNull(error.ErrorLocation,
                    $"Error location was expected to be null, but contained the following: {error.ErrorLocation}");
            }
        }

        [TestMethod]
        public void FulcrumException_TryAgain()
        {
            var exceptionMessage = Guid.NewGuid().ToString();
            var exception = new FulcrumTryAgainException(exceptionMessage);
            try
            {
                throw exception;
            }
            catch (FulcrumException e)
            {
                Assert.IsNotNull(e.StackTrace);
                var error = ExceptionConverter.ToFulcrumError(exception);
                Assert.IsNotNull(error);
                Assert.AreEqual(exceptionMessage, error.TechnicalMessage);
                Assert.AreEqual("Xlent.Fulcrum.TryAgain", error.Type);

                Assert.IsNull(error.ErrorLocation,
                    $"Error location was expected to be null, but contained the following: {error.ErrorLocation}");
            }
        }
    }
}
