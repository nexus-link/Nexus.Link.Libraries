﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Tests.Support;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.TestAssert
{
    [TestClass]
    public class TestInternalContract
    {

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TestFulcrumAssert));
        }

        [TestMethod]
        public void NullObject()
        {
            const string parameterName = "parameterName";
            try
            {
                object nullObject = null;
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.RequireNotNull(nullObject, parameterName);
              UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
               UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void NullString()
        {
            const string parameterName = "parameterName";
            try
            {
                string nullString = null;
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.RequireNotNullOrWhiteSpace(nullString, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void EmptyString()
        {
            const string parameterName = "parameterName";
            try
            {
                string emptyString = "";
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.RequireNotNullOrWhiteSpace(emptyString, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void WhitespaceString()
        {
            const string parameterName = "parameterName";
            try
            {
                string whitespaceString = "     \t";
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.RequireNotNullOrWhiteSpace(whitespaceString, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void Fail()
        {
            const string message = "fail with this string";
            try
            {
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.Fail(message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void False()
        {
            const string message = "fail because false";
            try
            {
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.Require(false, message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void FalseParameterExpression()
        {
            const string parameterName = "parameterName";
            try
            {
                const int value = 23;
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.Require(value, x => x != 23, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void FalseParameter()
        {
            const string parameterName = "parameterName";
            try
            {
                const int value = 0;
                // ReSharper disable once ExpressionIsAlwaysNull
                InternalContract.RequireNotDefaultValue(value, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void IsValidatedOk()
        {
            var validatable = new Validatable
            {
                Name = "Jim"
            };
            InternalContract.RequireValidated(validatable, nameof(validatable));
        }

        [TestMethod]
        public void IsValidatedFail()
        {
            try
            {
                var validatable = new Validatable();
                InternalContract.RequireValidated(validatable, nameof(validatable));
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage);
                var validationFailed = "ContractViolation: Validation failed";
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.StartsWith(validationFailed), $"Expected {nameof(fulcrumException.TechnicalMessage)}  to start with \"{validationFailed}\", but the message was \"{fulcrumException.TechnicalMessage}\".");
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains("Property Validatable.Name"),
                    $"TechnicalMessage: '{fulcrumException.TechnicalMessage}'");
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        #region Less Than Greater Than

        [TestMethod]
        public void LessThanFail()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireLessThan(1, 1, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void LessThanOk()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireLessThan(10, 1, parameterName);
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected no exception but got {e.Message}.");
            }
        }

        [TestMethod]
        public void LessThanOrEqualFail()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireLessThanOrEqualTo(1, 2, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void LessThanOrEqualOk()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireLessThanOrEqualTo(1, 1, parameterName);
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected no exception but got {e.Message}.");
            }
        }

        [TestMethod]
        public void GreaterThanFail()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireGreaterThan(1, 1, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void GreaterThanOk()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireGreaterThan(1, 2, parameterName);
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected no exception but got {e.Message}.");
            }
        }

        [TestMethod]
        public void GreaterThanOrEqualFail()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireGreaterThanOrEqualTo(2, 1, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void GreaterThanOrEqualOk()
        {
            const string parameterName = "parameterName";
            try
            {
                InternalContract.RequireGreaterThanOrEqualTo(1, 1, parameterName);
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected no exception but got {e.Message}.");
            }
        }

        #endregion
    }
}
