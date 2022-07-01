using System;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Misc;
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
            var expectedParentExecutionId = Guid.NewGuid().ToGuidString();
            var expectedExecutionId = Guid.NewGuid().ToGuidString();
            var expectedChildExecutionId = Guid.NewGuid().ToGuidString();
            var expectedChildExecutionDescription = Guid.NewGuid().ToString();
            var expectedExecutionIsAsynchronous = true;
            var expectedAsyncRequestId = Guid.NewGuid().ToString();
            var expectedAsyncPriority = 0.75;
            FulcrumApplication.Context.CallingClientName = expectedCallingClientName;
            FulcrumApplication.Context.ClientPrincipal =
                new GenericPrincipal(new GenericIdentity(expectedClientPrincipalName), new[] { "role1" });
            FulcrumApplication.Context.UserPrincipal =
                new GenericPrincipal(new GenericIdentity(expectedUserPrincipalName), new[] { "role2" });
            FulcrumApplication.Context.ClientTenant = expectedTenant;
            FulcrumApplication.Context.CorrelationId = expectedCorrelationId;
            FulcrumApplication.Context.LeverConfiguration = expectedLeverConfiguration;
            FulcrumApplication.Context.ParentExecutionId = expectedParentExecutionId;
            FulcrumApplication.Context.ExecutionId = expectedExecutionId;
            FulcrumApplication.Context.ChildExecutionId = expectedChildExecutionId;
            FulcrumApplication.Context.ChildRequestDescription = expectedChildExecutionDescription;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            FulcrumApplication.Context.ExecutionIsAsynchronous = expectedExecutionIsAsynchronous;
            FulcrumApplication.Context.AsyncRequestId = expectedAsyncRequestId;
            FulcrumApplication.Context.AsyncPriority = expectedAsyncPriority;


            UT.Assert.AreEqual(expectedCallingClientName, FulcrumApplication.Context.CallingClientName);
            UT.Assert.AreEqual(expectedClientPrincipalName, FulcrumApplication.Context.ClientPrincipal.Identity.Name);
            UT.Assert.AreEqual(expectedCorrelationId, FulcrumApplication.Context.CorrelationId);
            UT.Assert.AreEqual(expectedUserPrincipalName, FulcrumApplication.Context.UserPrincipal.Identity.Name);
            UT.Assert.AreEqual(expectedTenant, FulcrumApplication.Context.ClientTenant);
            UT.Assert.AreEqual(expectedLeverConfiguration, FulcrumApplication.Context.LeverConfiguration);
            UT.Assert.AreEqual(expectedParentExecutionId, FulcrumApplication.Context.ParentExecutionId);
            UT.Assert.AreEqual(expectedExecutionId, FulcrumApplication.Context.ExecutionId);
            UT.Assert.AreEqual(expectedChildExecutionId, FulcrumApplication.Context.ChildExecutionId);
            UT.Assert.AreEqual(expectedChildExecutionDescription, FulcrumApplication.Context.ChildRequestDescription);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            UT.Assert.AreEqual(expectedExecutionIsAsynchronous, FulcrumApplication.Context.ExecutionIsAsynchronous);
            UT.Assert.AreEqual(expectedAsyncRequestId, FulcrumApplication.Context.AsyncRequestId);
            UT.Assert.AreEqual(expectedAsyncPriority, FulcrumApplication.Context.AsyncPriority);
        }

    }
}
