﻿using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Platform.Configurations
{
    /// <summary>
    /// Contains what is needed in a convenience class for getting configurations and tokens.
    /// </summary>
    public interface ILeverServiceConfiguration
    {
        /// <summary>
        /// The name of the service that this service configuration is for.
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// The tenant behind this running service
        /// </summary>
        Tenant ServiceTenant { get; }

        /// <summary>
        /// Get the configuration for the current <see cref="ServiceTenant"/>.
        /// </summary>
        Task<ILeverConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the configuration for another tenant.
        /// </summary>
       Task<ILeverConfiguration> GetConfigurationForAsync(Tenant tenant, CancellationToken cancellationToken = default);
    }
}