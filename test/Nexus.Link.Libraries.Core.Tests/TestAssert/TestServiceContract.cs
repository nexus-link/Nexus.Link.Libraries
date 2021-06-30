using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Tests.Support;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.TestAssert
{
    [TestClass]
    public class TestServiceContract
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
                ServiceContract.RequireNotNull(nullObject, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void NullParameterNameIsOkWhenThereIsACustomMessage()
        {
            var value = "A";
            // ReSharper disable once ExpressionIsAlwaysNull
            ServiceContract.RequireAreEqual(value, value, null, "This assertion has a custom message, so null parameter name is OK");
        }

        [TestMethod]
        public void NullString()
        {
            const string parameterName = "parameterName";
            try
            {
                string nullString = null;
                // ReSharper disable once ExpressionIsAlwaysNull
                ServiceContract.RequireNotNullOrWhiteSpace(nullString, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireNotNullOrWhiteSpace(emptyString, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireNotNullOrWhiteSpace(whitespaceString, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void AreEqualOk()
        {
            const string parameterName = "parameterName";
            ServiceContract.RequireAreEqual(10, 5 * 2, parameterName);
        }

        [TestMethod]
        public void AreEqualFail()
        {
            const string parameterName = "parameterName";
            try
            {
                ServiceContract.RequireAreEqual("Knoll", "Tott", parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void AreNotEqualOk()
        {
            const string parameterName = "parameterName";
            ServiceContract.RequireAreNotEqual("Knoll","Tott", parameterName);
        }

        [TestMethod]
        public void AreNotEqualFail()
        {
            const string parameterName = "parameterName";
            try
            {
                ServiceContract.RequireAreNotEqual(10, 2*5, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(parameterName));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        internal enum TestEnum
        {
            Value
        }

        [TestMethod]
        public void InEnumOk()
        {
            const string parameterName = "parameterName";
            ServiceContract.RequireInEnumeration(typeof(TestEnum), "Value", parameterName);
        }

        [TestMethod]
        public void InEnumFail()
        {
            const string parameterName = "parameterName";
            try
            {
                ServiceContract.RequireInEnumeration(typeof(TestEnum), "Unknown", parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.Fail(message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.Require(false, message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.Require(value, x => x != 23, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireNotDefaultValue(value, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
            ServiceContract.RequireValidated(validatable, nameof(validatable));
        }

        [TestMethod]
        public void IsValidatedFail()
        {
            try
            {
                var validatable = new Validatable();
                ServiceContract.RequireValidated(validatable, nameof(validatable));
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireLessThan(1, 1, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireLessThan(10, 1, parameterName);
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
                ServiceContract.RequireLessThanOrEqualTo(1, 2, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireLessThanOrEqualTo(1, 1, parameterName);
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
                ServiceContract.RequireGreaterThan(1, 1, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireGreaterThan(1, 2, parameterName);
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
                ServiceContract.RequireGreaterThanOrEqualTo(2, 1, parameterName);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumServiceContractException fulcrumException)
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
                ServiceContract.RequireGreaterThanOrEqualTo(1, 1, parameterName);
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected no exception but got {e.Message}.");
            }
        }

        #endregion
    }
}
