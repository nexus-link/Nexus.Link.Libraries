using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Health.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Shouldly;

namespace Nexus.Link.Libraries.Core.Tests.Health
{
    [TestClass]
    public class PotentialHealthProblemTests
    {

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(PotentialHealthProblemTests)); 
            FulcrumApplication.Setup.HealthTracker.ResetAllHealthProblems();
        }

        [DataTestMethod]
        [DataRow("a", "message1")]
        [DataRow(null, "message2")]
        public void Fail_Given_FirstFail_Gives_Created(string environment, string message)
        {
            // Arrange
            var expectedTenant = environment == null ? null : new Tenant("organization", environment);
            var expectedId = Guid.NewGuid().ToGuidString();
            var expectedResource = Guid.NewGuid().ToString();
            var expectedTitle = Guid.NewGuid().ToString();
            var iut = new PotentialHealthProblem(expectedId, expectedResource, expectedTitle)
            {
                Tenant = expectedTenant
            };

            // Act
            iut.Fail(message);

            // Assert
            var problems = FulcrumApplication.Setup.HealthTracker.GetAllHealthProblems();
            problems.Count.ShouldBe(1);
            problems[0].Id.ShouldBe(expectedId);
            problems[0].Tenant.ShouldBe(expectedTenant);
            problems[0].Resource.ShouldBe(expectedResource);
            problems[0].Title.ShouldBe(expectedTitle);
            problems[0].MessageCounters.Count.ShouldBe(1);
            problems[0].MessageCounters.Keys.First().ShouldBe(message);
            problems[0].MessageCounters.Values.First().ShouldBe(1);
        }

        [DataTestMethod]
        [DataRow("a", "message1")]
        [DataRow(null, "message2")]
        public void Fail_Given_FirstFailWithException_Gives_Created(string environment, string message)
        {
            // Arrange
            var expectedTenant = environment == null ? null : new Tenant("organization", environment);
            var expectedId = Guid.NewGuid().ToGuidString();
            var expectedResource = Guid.NewGuid().ToString();
            var expectedTitle = Guid.NewGuid().ToString();
            var iut = new PotentialHealthProblem(expectedId, expectedResource, expectedTitle)
            {
                Tenant = expectedTenant
            };

            // Act
            iut.Fail(new Exception(message));

            // Assert
            var problems = FulcrumApplication.Setup.HealthTracker.GetAllHealthProblems();
            problems.Count.ShouldBe(1);
            problems[0].Id.ShouldBe(expectedId);
            problems[0].Tenant.ShouldBe(expectedTenant);
            problems[0].Resource.ShouldBe(expectedResource);
            problems[0].Title.ShouldBe(expectedTitle);
            problems[0].MessageCounters.Count.ShouldBe(1);
            problems[0].MessageCounters.Keys.First().ShouldNotBe(message);
            problems[0].MessageCounters.Keys.First().ShouldContain(message);
            problems[0].MessageCounters.Values.First().ShouldBe(1);
        }

        [DataTestMethod]
        [DataRow("a")]
        [DataRow(null)]
        public void Fail_Given_SuccessAfterFail_Gives_Removed(string environment)
        {
            // Arrange
            var expectedTenant = environment == null ? null : new Tenant("organization", environment);
            var expectedId = Guid.NewGuid().ToGuidString();
            var expectedResource = Guid.NewGuid().ToString();
            var expectedTitle = Guid.NewGuid().ToString();
            var iut = new PotentialHealthProblem(expectedId, expectedResource, expectedTitle)
            {
                Tenant = expectedTenant
            };
            iut.Fail("message");
            iut = new PotentialHealthProblem(expectedId, expectedResource, expectedTitle)
            {
                Tenant = expectedTenant
            };
            iut.Success();            // Act


            // Assert
            var problems = FulcrumApplication.Setup.HealthTracker.GetAllHealthProblems();
            problems.Count.ShouldBe(0);
        }

        [DataTestMethod]
        [DataRow(null, 1)]
        [DataRow(null, 2)]
        [DataRow(null, 10)]
        [DataRow("a", 1)]
        [DataRow("a", 2)]
        [DataRow("a", 10)]
        public void Fail_Given_Existing_Gives_Updated(string environment, int expectedCount)
        {
            // Arrange
            var expectedTenant = environment == null ? null : new Tenant("organization", environment);
            var expectedId = Guid.NewGuid().ToGuidString();
            var expectedResource = Guid.NewGuid().ToString();
            var expectedTitle = Guid.NewGuid().ToString();
            var iut = new PotentialHealthProblem(expectedId, expectedResource, expectedTitle)
            {
                Tenant = expectedTenant
            };
            var expectedMessage = Guid.NewGuid().ToString();

            // Act
            for (var i = 0; i < expectedCount; i++)
            {
                iut.Fail(expectedMessage);
            }

            // Assert
            var problems = FulcrumApplication.Setup.HealthTracker.GetAllHealthProblems();
            problems.Count.ShouldBe(1);
            problems[0].Id.ShouldBe(expectedId);
            problems[0].Tenant.ShouldBe(expectedTenant);
            problems[0].Resource.ShouldBe(expectedResource);
            problems[0].Title.ShouldBe(expectedTitle);
            problems[0].MessageCounters.Count.ShouldBe(1);
            problems[0].MessageCounters.Keys.First().ShouldBe(expectedMessage);
            problems[0].MessageCounters.Values.First().ShouldBe(expectedCount);
        }
    }
}
