using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Health.Logic;
using Nexus.Link.Libraries.Core.Health.Model;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nexus.Link.Libraries.Core.Tests.Health
{
    [TestClass]
    public class HealthTest
    {
        private static readonly Tenant Tenant = new Tenant("Super", "Mario");
        private IResourceHealth2 _goombaResource;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(HealthTest));
            FulcrumApplication.Setup.HealthTracker.ResetAllHealthProblems();
            _goombaResource = new GoombaResource();
        }

        [TestMethod]
        public async Task TestWithIResourceHealth2()
        {
            const string resource = "Goomba";
            var aggregator = new ResourceHealthAggregator2(Tenant, resource);
            await aggregator.AddResourceHealthAsync("DB", _goombaResource);
            var aggregatedHealthResponse = aggregator.GetAggregatedHealthResponse();
            UT.Assert.AreEqual(resource, aggregatedHealthResponse.Name);
            UT.Assert.IsNotNull(aggregatedHealthResponse.Resources);
            UT.Assert.AreEqual(1, aggregatedHealthResponse.Resources.Count);
            UT.Assert.AreEqual(GoombaResource.Name, aggregatedHealthResponse.Resources.First().Resource);
            UT.Assert.AreEqual(HealthInfo.StatusEnum.Ok, aggregatedHealthResponse.Resources.First().Status);
        }

        [TestMethod]
        public async Task TestWithDelegate()
        {
            const string resource = "Koopa";
            var aggregator = new ResourceHealthAggregator2(Tenant, resource);
            await aggregator.AddResourceHealthAsync("DB", HealthDelegateMethod);
            var aggregatedHealthResponse = aggregator.GetAggregatedHealthResponse();
            UT.Assert.AreEqual(resource, aggregatedHealthResponse.Name);
            UT.Assert.IsNotNull(aggregatedHealthResponse.Resources);
            UT.Assert.AreEqual(1, aggregatedHealthResponse.Resources.Count);
            UT.Assert.AreEqual("KOOPA", aggregatedHealthResponse.Resources.First().Resource);
            UT.Assert.AreEqual(HealthInfo.StatusEnum.Ok, aggregatedHealthResponse.Resources.First().Status);
        }

        private static Task<HealthInfo> HealthDelegateMethod(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HealthInfo
            {
                Resource = "KOOPA",
                Status = HealthInfo.StatusEnum.Ok,
                Message = "Troopa"
            });
        }
    }

    public class GoombaResource : IResourceHealth2
    {
        public const string Name = "GOOMBA";

        public Task<HealthInfo> GetResourceHealth2Async(Tenant tenant, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HealthInfo
            {
                Resource = Name,
                Status = HealthInfo.StatusEnum.Ok,
                Message = "Kuribo"
            });
        }
    }
}
