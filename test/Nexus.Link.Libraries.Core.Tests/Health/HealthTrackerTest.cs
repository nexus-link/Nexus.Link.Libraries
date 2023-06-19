using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Health.Logic;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Shouldly;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Health
{
    [TestClass]
    public class HealthTrackerTest
    {
        private static readonly Tenant Tenant = new Tenant("Super", "Mario");
        private IHealthTracker _iut;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(HealthTrackerTest));
            _iut = FulcrumApplication.Setup.HealthTracker;
            _iut.ResetAllProblemStates();
        }

        [DataTestMethod]
        [DataRow("id1", "a")]
        [DataRow("id1", null)]
        public void GetProblemState_Given_Empty_Gives_Null(string id, string environment)
        {
            // Arrange
            var expectedTenant = environment == null ? null : new Tenant("organization", environment);

            // Act
            var state = _iut.GetProblemState(id, expectedTenant);

            // Assert
            state.ShouldBeNull();
        }

        [DataTestMethod]
        [DataRow("a")]
        [DataRow(null)]
        public void GetProblemState_Given_Existing_Gives_Found(string environment)
        {
            // Arrange
            var expectedTenant = environment == null ? null : new Tenant("organization", environment);
            var expectedId = Guid.NewGuid().ToGuidString();
            var expectedResource = Guid.NewGuid().ToString();
            var expectedTitle = Guid.NewGuid().ToString();
            var savedState = new ProblemState(expectedId, expectedResource, expectedTitle, expectedTenant);
            _iut.SetProblemState(savedState);

            // Act
            var readState = _iut.GetProblemState(savedState.Id, savedState.Tenant);

            // Assert
            readState.ShouldNotBeNull();
            readState.Id.ShouldBe(expectedId);
            readState.Tenant.ShouldBe(expectedTenant);
            readState.Resource.ShouldBe(expectedResource);
            readState.Title.ShouldBe(expectedTitle);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(10)]
        public void AddError_Given_RepeatedSameMesage_Gives_CorrectCount(int expectedCount)
        {
            // Arrange
            const string id = "id1";
            var expectedMessage = Guid.NewGuid().ToString();
            var state = new ProblemState(id, "resource", "title");
            _iut.SetProblemState(state);

            // Act
            for (var i = 0; i < expectedCount; i++)
            {
                state = _iut.GetProblemState(id);
                state.AddError(expectedMessage);
                _iut.SetProblemState(state);
            }

            // Assert
            var readState = _iut.GetProblemState(id);
            readState.ErrorMessages.Count.ShouldBe(1);
            readState.ErrorMessages.ContainsKey(expectedMessage).ShouldBeTrue();
            readState.ErrorMessages[expectedMessage].ShouldBe(expectedCount);
        }

        [DataTestMethod]
        [DataRow(null, 1)]
        [DataRow("a", 0)]
        [DataRow("b", 1)]
        [DataRow("c", 2)]
        public void GetProblems_Given_MixedTenants_Gives_OwnTenantCount(string environment, int expectedCount)
        {
            // Arrange
            const string id = "id1";
            // No tenant
            var state = new ProblemState("id1", "resource", "title");
            _iut.SetProblemState(state);
            // Tenant b
            state = new ProblemState("id1", "resource", "title", new Tenant("o", "b"));
            _iut.SetProblemState(state);
            // Tenant c
            state = new ProblemState("id1", "resource", "title", new Tenant("o", "c"));
            _iut.SetProblemState(state);
            state = new ProblemState("id2", "resource", "title", new Tenant("o", "c"));
            _iut.SetProblemState(state);
            var tenant = environment == null ? null : new Tenant("o", environment);

            // Act
            var problems = _iut.GetProblems(tenant);

            // Assert
            problems.Count.ShouldBe(expectedCount);
        }

        [TestMethod]
        public void GetAllProblems_Given_MixedTenants_Gives_All()
        {
            // Arrange
            const string id = "id1";
            // No tenant
            var state = new ProblemState("id1", "resource", "title");
            _iut.SetProblemState(state);
            // Tenant b
            state = new ProblemState("id1", "resource", "title", new Tenant("o", "b"));
            _iut.SetProblemState(state);
            // Tenant c
            state = new ProblemState("id1", "resource", "title", new Tenant("o", "c"));
            _iut.SetProblemState(state);
            state = new ProblemState("id2", "resource", "title", new Tenant("o", "c"));
            _iut.SetProblemState(state);

            // Act
            var problems = _iut.GetAllProblems();

            // Assert
            problems.Count.ShouldBe(4);
        }

        [TestMethod]
        public void ResetProblemState_Given_Delete_Gives_Removed()
        {
            // Arrange
            const string id = "id1";
            // No tenant
            var state = new ProblemState("id1", "resource", "title");
            _iut.SetProblemState(state);
            // Tenant b
            var bTenant = new Tenant("o", "b");
            state = new ProblemState("id1", "resource", "title", bTenant);
            _iut.SetProblemState(state);
            // Tenant c
            var cTenant = new Tenant("o", "c");
            state = new ProblemState("id1", "resource", "title", cTenant);
            _iut.SetProblemState(state);
            state = new ProblemState("id2", "resource", "title", cTenant);
            _iut.SetProblemState(state);
            var problems = _iut.GetAllProblems();
            problems.Count.ShouldBe(4);

            // Act
            _iut.ResetProblemState("id1");
            _iut.ResetProblemState("id2", cTenant);

            // Assert
            problems = _iut.GetAllProblems();
            problems.Count.ShouldBe(2);
        }

        [TestMethod]
        public void ResetAllProblemStates_Given_MixedTenants_Gives_AllRemoved()
        {
            // Arrange
            const string id = "id1";
            // No tenant
            var state = new ProblemState("id1", "resource", "title");
            _iut.SetProblemState(state);
            // Tenant b
            state = new ProblemState("id1", "resource", "title", new Tenant("o", "b"));
            _iut.SetProblemState(state);
            // Tenant c
            state = new ProblemState("id1", "resource", "title", new Tenant("o", "c"));
            _iut.SetProblemState(state);
            state = new ProblemState("id2", "resource", "title", new Tenant("o", "c"));
            _iut.SetProblemState(state);
            var problems = _iut.GetAllProblems();
            problems.Count.ShouldBe(4);

            // Act
            _iut.ResetAllProblemStates();

            // Assert
            problems = _iut.GetAllProblems();
            problems.Count.ShouldBe(0);
        }
    }
}
