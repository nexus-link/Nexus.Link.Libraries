﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Tests.Support;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.TestAssert
{
    [TestClass]
    public class TestFulcrumAssert
    {
        private static readonly string Namespace = typeof(TestFulcrumAssert).Namespace;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TestFulcrumAssert));
        }

        [TestMethod]
        public void Fail()
        {
            const string message = "A random message";
            try
            {
                FulcrumAssert.Fail($"{Namespace}: D978C43E-D131-4663-A74A-FDF3EA4ED526", message);
              UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
               UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void IsTrueAssertionOk()
        {
            FulcrumAssert.IsTrue(true, $"{Namespace}: 5A4893CD-F20C-456B-BE56-AECAA592D022");
        }

        [TestMethod]
        public void IsTrueAssertionFail()
        {
            const string message = "A random message";
            try
            {
                FulcrumAssert.IsTrue(false, $"{Namespace}: D6945A7A-7ACF-44D8-89B1-4989D721CB22", message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void IsNotNullAssertionOk()
        {
            FulcrumAssert.IsNotNull(new object(), $"{Namespace}: 70FC66B7-618C-4C77-8A62-DCA5B6C45A13");
        }

        [TestMethod]
        public void IsNotNullAssertionFail()
        {
            const string message = "A random message";
            try
            {
                FulcrumAssert.IsNotNull(null, $"{Namespace}: 095DD6FC-BC21-42AF-8FDC-7AFEB391D9F6", message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void IsNotNullAssertionFailNoLocation()
        {
            const string message = "A random message";
            try
            {
                FulcrumAssert.IsNotNull(null, null, message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void IsNotNullOrWhiteSpaceAssertionOk()
        {
            FulcrumAssert.IsNotNullOrWhiteSpace("NotEmpty", $"{Namespace}: 5CA88FE7-BD6D-4FC8-82CE-2EDBC6517F0C", "311F3E2E-748B-4EB1-8420-09938D8A8AA7");
        }

        [TestMethod]
        public void IsNotNullOrWhiteSpaceAssertionFail()
        {
            const string message = "A random message";
            try
            {
                FulcrumAssert.IsNotNullOrWhiteSpace("  ", $"{Namespace}: B46B96EA-2919-4872-826D-967930DFA2B5", message);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void AreEqualAssertionOk()
        {
            FulcrumAssert.AreEqual(10, 5 * 2, $"{Namespace}: 6BC20889-87A4-450A-A326-D352A2FEBD81");
        }

        [TestMethod]
        public void AreEqualAssertionFail()
        {
            const string message = "A random message";
            try
            {
                FulcrumAssert.AreEqual("Knoll", "Tott", $"{Namespace}: 0F718BCF-B831-40D7-A172-8A4293F58EF6");
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        internal enum TestEnum
        {
            Value1,Value2
        }

        [TestMethod]
        public void AreEqualEnumAssertionOk()
        {
            FulcrumAssert.AreEqual(TestEnum.Value1, TestEnum.Value1, $"{Namespace}: 6BC20889-87A4-450A-A326-D352A2FEBD81");
        }

        [TestMethod]
        public void AreEqualEnumAssertionFail()
        {
            const string message = "A random message";
            try
            {
                FulcrumAssert.AreEqual(TestEnum.Value1, TestEnum.Value2, $"{Namespace}: 0F718BCF-B831-40D7-A172-8A4293F58EF6");
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage.Contains(message));
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void InEnumAssertionOk()
        {
            FulcrumAssert.IsInEnumeration(typeof(TestEnum), "Value1");
        }

        [TestMethod]
        public void InEnumAssertionFail()
        {
            try
            {
                FulcrumAssert.IsInEnumeration(typeof(TestEnum), "Unknown");
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException)
            {
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

        [TestMethod]
        public void AreEqualAssertionFailWithCodeLocation()
        {
            const string message = "A random message";
            var codeLocation = CodeLocation.AsString();
            try
            {
                FulcrumAssert.AreEqual("Knoll", "Tott", codeLocation);
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage.Contains(message));
                UT.Assert.AreEqual(codeLocation, fulcrumException.ErrorLocation);
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
            FulcrumAssert.IsValidated(validatable, $"{Namespace}: DCB46FEB-9347-47CC-9307-10702F8026E4");
        }

        [TestMethod]
        public void IsValidatedFail()
        {
            try
            {
                var validatable = new Validatable();
                FulcrumAssert.IsValidated(validatable, CodeLocation.AsString());
                UT.Assert.Fail("An exception should have been thrown");
            }
            catch (FulcrumAssertionFailedException fulcrumException)
            {
                UT.Assert.IsNotNull(fulcrumException.TechnicalMessage);
                UT.Assert.IsTrue(fulcrumException.TechnicalMessage.StartsWith("Expected validation to pass (Property Validatable.Name"), 
                    $"TechnicalMessage: '{fulcrumException.TechnicalMessage}'");
            }
            catch (Exception e)
            {
                UT.Assert.Fail($"Expected a specific FulcrumException but got {e.GetType().FullName}.");
            }
        }

    }
}
