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
        public void All_Given_NotNull_Gives_Ok()
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
        public void GreaterThanOrEqualTo_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeGreaterThanOrEqualTo5 = 3
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void GreaterThanOrEqualToProperty_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                MustBeGreaterThanOrEqualToOtherProperty = 3
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void MatchRegularExpression_Given_Fail_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                MatchRegularExpressionAndNotNull = "Volvo"
            };

            FulcrumAssert.IsValidated(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void MatchRegularExpression_Given_Null_Gives_Exception()
        {
            // Arrange
            var entity = new TestEntity
            {
                MatchRegularExpressionAndNotNull = null
            };

            FulcrumAssert.IsValidated(entity);
        }
    }

    public class TestEntity
    {
        [Validation.NotNull] 
        public string NotNull { get; set; } = "";

        [Validation.GreaterThanOrEqualTo(5)]
        public int MustBeGreaterThanOrEqualTo5 { get; set; } = 5;

        [Validation.GreaterThanOrEqualToProperty(nameof(OtherProperty))]
        public int MustBeGreaterThanOrEqualToOtherProperty { get; set; } = 5;

        public int OtherProperty { get; set; } = 5;

        [Validation.NotNull]
        [Validation.MatchRegularExpression("Sa+b")]
        public string MatchRegularExpressionAndNotNull { get; set; } = "Saab";

        [Hint.OptimisticConcurrencyControl]
        public string Etag { get; set; }
    }
}
