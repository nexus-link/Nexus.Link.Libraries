using System.Collections.Generic;

namespace Nexus.Link.Libraries.Core.Platform.ServiceMetas
{
    /// <summary>
    /// Information about a service, like it's name and dependencies on other services
    /// </summary>
    public class ServiceInformation
    {
        /// <summary>
        /// A server-to-server name of the service
        /// </summary>
        public string TechnicalName { get; set; }

        /// <summary>
        /// A descriptive name of the service
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// A list of services that this service is dependant on
        /// </summary>
        public List<ServiceDependency> Dependencies { get; set; }
    }
}
