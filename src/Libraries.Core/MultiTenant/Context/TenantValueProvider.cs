using System;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.MultiTenant.Context
{
    /// <summary>
    /// Stores CorrelationId in the execution context.
    /// </summary>
    [Obsolete("Use FulcrumApplication.Context.ClientTenant.", true)]
    public class TenantValueProvider
    {
        private static bool _firstTime = true;
        private readonly OneValueProvider<Tenant> _clientTenant;

        /// <summary>
        /// Constructor
        /// </summary>
        public TenantValueProvider()
        {
            if (_firstTime)
            {
                FulcrumApplication.Validate();
                _firstTime = false;
            }
            _clientTenant = new OneValueProvider<Tenant>(FulcrumApplication.Context.ValueProvider, "TenantId");
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public Tenant ClientTenant
        {
            get => _clientTenant.GetValue();
            set => _clientTenant.SetValue(value);
        }
    }
}
