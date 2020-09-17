using System;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Context
{
    [TestClass]
    public class TestNexusContext
    {
        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(TestNexusContext));
        }

        [TestCleanup]
        public void Cleanup()
        {
            FulcrumApplication.Context.ValueProvider.Reset();
        }

        [TestMethod]
        public void ValuesDoesNotClash()
        {
            var expectedCallingClientName = Guid.NewGuid().ToString();
            var expectedClientPrincipalName = Guid.NewGuid().ToString();
            var expectedCorrelationId = Guid.NewGuid().ToString();
            var expectedUserPrincipalName = Guid.NewGuid().ToString();
            var expectedTenant = new Tenant(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var expectedLeverConfiguration = new Mock<ILeverConfiguration>().Object;
            FulcrumApplication.Context.CallingClientName = expectedCallingClientName;
            FulcrumApplication.Context.ClientPrincipal =
                new GenericPrincipal(new GenericIdentity(expectedClientPrincipalName), new[] { "role1" });
            FulcrumApplication.Context.UserPrincipal =
                new GenericPrincipal(new GenericIdentity(expectedUserPrincipalName), new[] { "role2" });
            FulcrumApplication.Context.ClientTenant = expectedTenant;
            FulcrumApplication.Context.CorrelationId = expectedCorrelationId;
            FulcrumApplication.Context.LeverConfiguration = expectedLeverConfiguration;

            UT.Assert.AreEqual(expectedCallingClientName, FulcrumApplication.Context.CallingClientName);
            UT.Assert.AreEqual(expectedClientPrincipalName, FulcrumApplication.Context.ClientPrincipal.Identity.Name);
            UT.Assert.AreEqual(expectedCorrelationId, FulcrumApplication.Context.CorrelationId);
            UT.Assert.AreEqual(expectedUserPrincipalName, FulcrumApplication.Context.UserPrincipal.Identity.Name);
            UT.Assert.AreEqual(expectedTenant, FulcrumApplication.Context.ClientTenant);
            UT.Assert.AreEqual(expectedLeverConfiguration, FulcrumApplication.Context.LeverConfiguration);
        }

    }
}
