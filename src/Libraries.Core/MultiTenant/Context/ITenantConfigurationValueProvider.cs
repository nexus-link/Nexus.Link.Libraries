using System;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Core.MultiTenant.Context
{
    /// <summary>
    /// Adds Tenant and LeverConfiguration to what <see cref="ICorrelationIdValueProvider"/> provides.
    /// </summary>
    [Obsolete("Use TenantConfigurationValueProvider directly.", true)]
    public interface ITenantConfigurationValueProvider
    {
        /// <summary>
        /// The current Tenant.
        /// </summary>
        Tenant Tenant { get; set; }

        /// <summary>
        /// The current configuration.
        /// </summary>
        ILeverConfiguration LeverConfiguration { get; set; }

        /// <summary>
        /// Gets the calling client name, provided that it has been setup, e.g. by Xlent.Lever.Authentication.Sdk.Handlers.TokenValidationHandler
        /// </summary>
        string CallingClientName { get; set; }
    }
}