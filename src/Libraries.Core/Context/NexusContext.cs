using System;
using System.Security.Principal;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Core.Context
{
    public class NexusContext
    {
        public IContextValueProvider ValueProvider { get; }

        private readonly OneValueProvider<string> _correlationId;
        private readonly OneValueProvider<string> _callingClientName;
        private readonly OneValueProvider<Tenant> _clientTenant;
        private readonly OneValueProvider<ILeverConfiguration> _leverConfiguration;
        private readonly OneValueProvider<IPrincipal> _clientPrincipal;
        private readonly OneValueProvider<IPrincipal> _userPrincipal;
        private readonly OneValueProvider<bool> _isInBatchLogger;

        public NexusContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _correlationId = new OneValueProvider<string>(ValueProvider, "NexusCorrelationId");
            _callingClientName = new OneValueProvider<string>(ValueProvider, "CallingClientName");
            _clientTenant = new OneValueProvider<Tenant>(ValueProvider, "TenantId");
            _leverConfiguration = new OneValueProvider<ILeverConfiguration>(ValueProvider, "LeverConfigurationId");
            _clientPrincipal = new OneValueProvider<IPrincipal>(ValueProvider, "ClientPrincipal");
            _userPrincipal = new OneValueProvider<IPrincipal>(ValueProvider, "UserPrincipal");
            _isInBatchLogger = new OneValueProvider<bool>(ValueProvider, "IsInBatchLogger");
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public Guid ContextId => ValueProvider.ContextId;

        /// <summary>
        /// Access the context data
        /// </summary>
        public string CorrelationId
        {
            get => _correlationId.GetValue();
            set => _correlationId.SetValue(value);
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public string CallingClientName
        {
            get => _callingClientName.GetValue();
            set => _callingClientName.SetValue(value);
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public Tenant ClientTenant
        {
            get => _clientTenant.GetValue();
            set => _clientTenant.SetValue(value);
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public ILeverConfiguration LeverConfiguration
        {
            get => _leverConfiguration.GetValue();
            set => _leverConfiguration.SetValue(value);
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public IPrincipal ClientPrincipal
        {
            get => _clientPrincipal.GetValue();
            set => _clientPrincipal.SetValue(value);
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public IPrincipal UserPrincipal
        {
            get => _userPrincipal.GetValue();
            set => _userPrincipal.SetValue(value);
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public bool IsInBatchLogger
        {
            get => _isInBatchLogger.GetValue();
            set => _isInBatchLogger.SetValue(value);
        }

    }
}