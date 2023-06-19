using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Health.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Shouldly;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Health
{
    [TestClass]
    public class HealthTrackerTest
    {
        private IHealthTracker _iut;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(HealthTrackerTest));
            _iut = FulcrumApplication.Setup.HealthTracker;
            _iut.ResetAllHealthProblems();
        }

        [DataTestMethod]
        [DataRow(null, 1)]
        [DataRow("a", 0)]
        [DataRow("b", 1)]
        [DataRow("c", 2)]
        public void GetProblems_Given_MixedTenants_Gives_OwnTenantCount(string environment, int expectedCount)
        {
            // Arrange
            // No tenant
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message");
            // Tenant b
            var bTenant = new Tenant("o", "b");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", bTenant);
            // Tenant c
            var cTenant = new Tenant("o", "c");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", cTenant);
            _iut.AddHealthProblemMessage("id2", "resource", "title", "message", cTenant);
            var tenant = environment == null ? null : new Tenant("o", environment);

            // Act
            var problems = _iut.GetHealthProblems(tenant);

            // Assert
            problems.Count.ShouldBe(expectedCount);
        }

        [TestMethod]
        public void GetAllProblems_Given_MixedTenants_Gives_All()
        {
            // Arrange
            // No tenant
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message");
            // Tenant b
            var bTenant = new Tenant("o", "b");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", bTenant);
            // Tenant c
            var cTenant = new Tenant("o", "c");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", cTenant);
            _iut.AddHealthProblemMessage("id2", "resource", "title", "message", cTenant);

            // Act
            var problems = _iut.GetAllHealthProblems();

            // Assert
            problems.Count.ShouldBe(4);
        }

        [TestMethod]
        public void ResetProblemState_Given_Delete_Gives_Removed()
        {
            // Arrange
            // No tenant
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message");
            // Tenant b
            var bTenant = new Tenant("o", "b");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", bTenant);
            // Tenant c
            var cTenant = new Tenant("o", "c");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", cTenant);
            _iut.AddHealthProblemMessage("id2", "resource", "title", "message", cTenant);
            var problems = _iut.GetAllHealthProblems();
            problems.Count.ShouldBe(4);

            // Act
            _iut.ResetHealthProblem("id1");
            _iut.ResetHealthProblem("id2", cTenant);

            // Assert
            problems = _iut.GetAllHealthProblems();
            problems.Count.ShouldBe(2);
        }

        [TestMethod]
        public void ResetAllProblemStates_Given_MixedTenants_Gives_AllRemoved()
        {
            // Arrange
            // No tenant
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message");
            // Tenant b
            var bTenant = new Tenant("o", "b");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", bTenant);
            // Tenant c
            var cTenant = new Tenant("o", "c");
            _iut.AddHealthProblemMessage("id1", "resource", "title", "message", cTenant);
            _iut.AddHealthProblemMessage("id2", "resource", "title", "message", cTenant);
            var problems = _iut.GetAllHealthProblems();
            problems.Count.ShouldBe(4);

            // Act
            _iut.ResetAllHealthProblems();

            // Assert
            problems = _iut.GetAllHealthProblems();
            problems.Count.ShouldBe(0);
        }
    }
}
