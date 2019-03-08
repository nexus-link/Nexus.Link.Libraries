using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Health.Model
{
    /// <summary>
    /// Interface to be implemented by every controller for a service that should report their health.
    /// </summary>
    public interface IResourceHealth2
    {
        /// <summary>
        /// Get the health status for a specific <paramref name="tenant"/>.
        /// </summary>
        Task<HealthInfo> GetResourceHealth2Async(Tenant tenant);
    }
}
