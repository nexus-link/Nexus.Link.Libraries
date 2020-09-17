using System;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Core.MultiTenant.Context
{
    /// <summary>
    /// Stores values in the execution context which is unaffected by asynchronous code that switches threads or context. 
    /// </summary>
    /// <remarks>Updating values in a thread will not affect the value in parent/sibling threads</remarks>
    [Obsolete("Use FulcrumApplication.Context.* values.", true)]
    public class TenantConfigurationValueProvider
    {
        private readonly TenantValueProvider _tenantProvider;
        private readonly ConfigurationValueProvider _configurationProvider;
        private readonly CallingClientValueProvider _callingClientProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        public TenantConfigurationValueProvider()
        {
            _tenantProvider = new TenantValueProvider();
            _configurationProvider = new ConfigurationValueProvider();
            _callingClientProvider = new CallingClientValueProvider();
        }

        /// <summary>
        /// The current Tenant.
        /// </summary>
        public Tenant Tenant
        {
            get => _tenantProvider.ClientTenant;
            set => _tenantProvider.ClientTenant = value;
        }

        /// <summary>
        /// The current configuration.
        /// </summary>
        public ILeverConfiguration LeverConfiguration
        {
            get => _configurationProvider.LeverConfiguration;
            set => _configurationProvider.LeverConfiguration = value;
        }

        /// <summary>
        /// Gets the calling client name, provided that it has been setup, e.g. by Xlent.Lever.Authentication.Sdk.Handlers.TokenValidationHandler
        /// </summary>
        public string CallingClientName
        {
            get => _callingClientProvider.CallingClientName;
            set => _callingClientProvider.CallingClientName = value;
        }
    }
}
