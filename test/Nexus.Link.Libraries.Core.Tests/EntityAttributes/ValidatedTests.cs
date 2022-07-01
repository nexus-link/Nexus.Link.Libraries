using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.Libraries.Core.Tests.EntityAttributes
{
    [TestClass]
    public class ValidatedTests
    {
        [TestMethod]
        public void All_Given_Ok_Gives_Ok()
        {
            // Arrange
            var entity = new TestEntity();

            // 
            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotNull_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                NotNull = null
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Null_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                Null = "not null"
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotNullOrEmpty_Given_Fail_Gives_Exception(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                NotNullOrEmpty = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(" x")]
        [DataRow("x ")]
        [DataRow(" x ")]
        public void NotNullOrWhitespace_Given_Ok_Gives_Ok(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                NotNullOrWhitespace = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\t")]
        [DataRow("  ")]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotNullOrWhitespace_Given_Fail_Gives_Exception(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                NotNullOrWhitespace = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotDefault_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                NotDefault = 0
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("dange")]
        [DataRow("anger")]
        [DataRow("banger")]
        public void NotEqualTo_Given_Ok_Gives_Ok(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                NotEqualToDanger = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotEqualTo_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                NotEqualToDanger = "danger"
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotEqualToProperty_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                NotEqualToOtherProperty = 5
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(4)]
        [DataRow(5)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void GreaterThan_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeGreaterThan5 = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(3)]
        [DataRow(4)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void GreaterThanOrEqualTo_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeGreaterThanOrEqualTo5 = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(4)]
        [DataRow(5)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void GreaterThanProperty_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeGreaterThanOtherProperty = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(3)]
        [DataRow(4)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void GreaterThanOrEqualToProperty_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeGreaterThanOrEqualToOtherProperty = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(5)]
        [DataRow(6)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void LessThan_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeLessThan5 = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(6)]
        [DataRow(7)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void LessThanOrEqualTo_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeLessThanOrEqualTo5 = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(5)]
        [DataRow(6)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void LessThanProperty_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeLessThanOtherProperty = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(6)]
        [DataRow(7)]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void LessThanOrEqualToProperty_Given_Fail_Gives_Exception(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeLessThanOrEqualToOtherProperty = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("Sab")]
        // ReSharper disable once StringLiteralTypo
        [DataRow("Saaaab")]
        // ReSharper disable once StringLiteralTypo
        [DataRow("Asab")]
        public void MatchRegularExpression_Given_Ok_Gives_Ok(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MatchRegularExpressionCaseInsensitive = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow("Sacb")]
        [DataRow("Volvo")]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void MatchRegularExpression_Given_Fail_Gives_Exception(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                MatchRegularExpressionCaseInsensitive = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow(nameof(ColorEnum.Blue))]
        [DataRow(nameof(ColorEnum.Green))]
        [DataRow(nameof(ColorEnum.Red))]
        [DataRow(nameof(ColorEnum.Magenta))]
        public void InEnumeration_Given_Ok_Gives_Ok(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                StringInEnumerationForColors = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void InEnumeration_Given_StringFail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                StringInEnumerationForColors = "Banana"
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow((int)ColorEnum.Blue)]
        [DataRow((int)ColorEnum.Green)]
        [DataRow((int)ColorEnum.Red)]
        [DataRow((int)ColorEnum.Magenta)]
        public void InEnumeration_Given_IntegerOk_Gives_Ok(int actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                IntegerInEnumerationForColors = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void InEnumeration_Given_IntegerFail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                IntegerInEnumerationForColors = 1000
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void JsonString_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                JsonString = "Not json"
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow("lower")]
        [DataRow("swedish lower case å")]
        [DataRow("swedish lower case ä")]
        [DataRow("swedish lower case ö")]
        public void LowerCase_Given_Ok_Gives_Ok(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                LowerCase = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow("UPPER")]
        [DataRow("Mixed")]
        [DataRow("swedish upper case Å")]
        [DataRow("swedish upper case Ä")]
        [DataRow("swedish upper case Ö")]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void LowerCase_Given_Fail_Gives_Exception(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                JsonString = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow("UPPER")]
        [DataRow("SWEDISH UPPER CASE Å")]
        [DataRow("SWEDISH UPPER CASE Ä")]
        [DataRow("SWEDISH UPPER CASE Ö")]
        public void UpperCase_Given_Ok_Gives_Ok(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                UpperCase = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [DataTestMethod]
        [DataRow("lower")]
        [DataRow("Mixed")]
        [DataRow("SWEDISH LOWER CASE å")]
        [DataRow("SWEDISH LOWER CASE ä")]
        [DataRow("SWEDISH LOWER CASE ö")]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void UpperCase_Given_Fail_Gives_Exception(string actualValue)
        {
            // Arrange
            var entity = new TestEntity
            {
                JsonString = actualValue
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        public void Validate_Given_NoSubValidation_Gives_Ok()
        {
            // Arrange
            var entity = new TestEntity();
            entity.SubEntityWithoutValidation.NotNull = null;
            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Validate_Given_SubValidation_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity();
            entity.SubEntityWithValidation.NotNull = null;
            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Validate_Given_SubValidations_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity();
            entity.SubEntitiesWithValidation.Add(new SubTestEntity
            {
                NotNull = null
            });
            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotNullTriggerProperty_Given_NullAndTrue_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                Flag = true,
                NotNullIfFlag = null,
            };
            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        public void NotNullTriggerProperty_Given_NullAndFalse_Gives_Ok()
        {
            // Arrange
            var entity = new TestEntity
            {
                Flag = false,
                NotNullIfFlag = null,
            };
            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void NotNullInvertedTriggerProperty_Given_NullAndFalse_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                Flag = false,
                NotNullIfNotFlag = null,
            };
            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        public void NotNullInvertedTriggerProperty_Given_NullAndTrue_Gives_Ok()
        {
            // Arrange
            var entity = new TestEntity
            {
                Flag = true,
                NotNullIfNotFlag = null,
            };
            FulcrumAssert.IsValidated(entity);
        }
    }

    internal class TestEntity
    {
        [Validation.NotNull]
        public string NotNull { get; set; } = "";

        [Validation.Null]
        public string Null { get; set; } = null;

        [Validation.NotNullOrEmpty]
        public string NotNullOrEmpty { get; set; } = " ";

        [Validation.NotNullOrWhitespace]
        public string NotNullOrWhitespace { get; set; } = " a";

        [Validation.NotDefault]
        public int NotDefault { get; set; } = 1;

        [Validation.GreaterThan(5)]
        public int MustBeGreaterThan5 { get; set; } = 6;

        [Validation.NotEqualTo("danger")]
        public string NotEqualToDanger { get; set; } = "safe";

        [Validation.NotEqualToProperty(nameof(IntegerPropertyWithValue5))]
        public int NotEqualToOtherProperty { get; set; } = 6;

        [Validation.GreaterThanOrEqualTo(5)]
        public int MustBeGreaterThanOrEqualTo5 { get; set; } = 5;

        [Validation.GreaterThanProperty(nameof(IntegerPropertyWithValue5))]
        public int MustBeGreaterThanOtherProperty { get; set; } = 6;

        [Validation.GreaterThanOrEqualToProperty(nameof(IntegerPropertyWithValue5))]
        public int MustBeGreaterThanOrEqualToOtherProperty { get; set; } = 5;

        [Validation.LessThan(5)]
        public int MustBeLessThan5 { get; set; } = 4;

        [Validation.LessThanOrEqualTo(5)]
        public int MustBeLessThanOrEqualTo5 { get; set; } = 5;

        [Validation.LessThanProperty(nameof(IntegerPropertyWithValue5))]
        public int MustBeLessThanOtherProperty { get; set; } = 4;

        [Validation.LessThanOrEqualToProperty(nameof(IntegerPropertyWithValue5))]
        public int MustBeLessThanOrEqualToOtherProperty { get; set; } = 5;

        public int IntegerPropertyWithValue5 { get; set; } = 5;

        [Validation.MatchRegularExpression("sa+b", RegexOptions.IgnoreCase)]
        public string MatchRegularExpressionCaseInsensitive { get; set; } = "Saab";

        [Validation.InEnumeration(typeof(ColorEnum))]
        public string StringInEnumerationForColors { get; set; } = "Red";

        [Validation.InEnumeration(typeof(ColorEnum))]
        public int IntegerInEnumerationForColors { get; set; } = 0;

        [Validation.JSonString]
        public string JsonString { get; set; } = "\"Json\"";

        [Validation.LowerCase]
        public string LowerCase { get; set; } = "lower case";

        [Validation.UpperCase]
        public string UpperCase { get; set; } = "UPPER CASE";

        public SubTestEntity SubEntityWithoutValidation { get; set; } = new SubTestEntity();

        [Validation.Validate]
        public SubTestEntity SubEntityWithValidation { get; set; } = new SubTestEntity();

        [Validation.Validate]
        public List<SubTestEntity> SubEntitiesWithValidation { get; set; } =
            new List<SubTestEntity>() {new SubTestEntity()};

        public bool Flag { get; set; }

        [Validation.NotNull(TriggerPropertyName = nameof(Flag))]
        public string NotNullIfFlag { get; set; } = "not null";

        [Validation.NotNull(TriggerPropertyName = nameof(Flag), InvertedTrigger = true)]
        public string NotNullIfNotFlag { get; set; } = "not null";
    }

    internal class SubTestEntity
    {
        [Validation.NotNull]
        public string NotNull { get; set; } = "not null";
    }

    public enum ColorEnum
    {
        Red, Green, Blue, Magenta
    }
}
