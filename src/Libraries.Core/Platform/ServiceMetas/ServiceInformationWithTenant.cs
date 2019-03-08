using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Platform.ServiceMetas
{
    /// <inheritdoc />
    public class ServiceInformationWithTenant : ServiceInformation
    {
        /// <summary>
        /// The <see cref="Tenant"/> running the service itself.
        /// </summary>
        public Tenant ServiceTenant { get; set; }
    }
}
