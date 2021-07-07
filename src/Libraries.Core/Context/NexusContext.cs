using System;
using System.Security.Principal;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.AsyncProcesses;
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
        private readonly OneValueProvider<string> _nexusTestContext;
        private readonly OneValueProvider<bool> _executionIsAsynchronous;
        private readonly OneValueProvider<ExecutionContext> _asyncExecutionContext;

        public NexusContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _executionIsAsynchronous = new OneValueProvider<bool>(ValueProvider, "ExecutionIsAsynchronous");
            _asyncExecutionContext = new OneValueProvider<ExecutionContext>(ValueProvider, "AsyncExecutionContext");
            _correlationId = new OneValueProvider<string>(ValueProvider, "NexusCorrelationId");
            _callingClientName = new OneValueProvider<string>(ValueProvider, "CallingClientName");
            _clientTenant = new OneValueProvider<Tenant>(ValueProvider, "TenantId");
            _leverConfiguration = new OneValueProvider<ILeverConfiguration>(ValueProvider, "LeverConfigurationId");
            _clientPrincipal = new OneValueProvider<IPrincipal>(ValueProvider, "ClientPrincipal");
            _userPrincipal = new OneValueProvider<IPrincipal>(ValueProvider, "UserPrincipal");
            _isInBatchLogger = new OneValueProvider<bool>(ValueProvider, "IsInBatchLogger");
            _nexusTestContext = new OneValueProvider<string>(ValueProvider, "NexusTestContext");
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public Guid ContextId => ValueProvider.ContextId;

        /// <summary>
        /// If this is true, then the current execution is in an truly asynchronous context,
        /// i.e. the client is not waiting for the response, so we are for instance
        /// free to make asynchronous calls to other servers.
        /// </summary>
        public bool ExecutionIsAsynchronous
        {
            get => _executionIsAsynchronous.GetValue();
            set => _executionIsAsynchronous.SetValue(value);
        }

        /// <summary>
        /// If non-null, contains the information about all requests and their responses.
        /// </summary>
        public ExecutionContext AsyncExecutionContext
        {
            get => _asyncExecutionContext.GetValue();
            set => _asyncExecutionContext.SetValue(value);
        }

        /// <summary>
        /// The correlation id.
        /// </summary>
        public string CorrelationId
        {
            get => _correlationId.GetValue();
            set => _correlationId.SetValue(value);
        }

        /// <summary>
        /// The name of the calling client, as set in the authentication token
        /// </summary>
        public string CallingClientName
        {
            get => _callingClientName.GetValue();
            set => _callingClientName.SetValue(value);
        }

        /// <summary>
        /// The client tenant, as specified in the URL.
        /// </summary>
        public Tenant ClientTenant
        {
            get => _clientTenant.GetValue();
            set => _clientTenant.SetValue(value);
        }

        /// <summary>
        /// The client-tenant-specific configuration 
        /// </summary>
        public ILeverConfiguration LeverConfiguration
        {
            get => _leverConfiguration.GetValue();
            set => _leverConfiguration.SetValue(value);
        }

        /// <summary>
        /// The client principal from the authentication token
        /// </summary>
        public IPrincipal ClientPrincipal
        {
            get => _clientPrincipal.GetValue();
            set => _clientPrincipal.SetValue(value);
        }

        /// <summary>
        /// The user principal from the authentication token
        /// </summary>
        public IPrincipal UserPrincipal
        {
            get => _userPrincipal.GetValue();
            set => _userPrincipal.SetValue(value);
        }

        /// <summary>
        /// True if we are in the batch logger code
        /// </summary>
        public bool IsInBatchLogger
        {
            get => _isInBatchLogger.GetValue();
            set => _isInBatchLogger.SetValue(value);
        }

        /// <summary>
        /// If non-null, caller is requesting test mode, and the value can be interpreted as a test context.
        /// Propagated as the Nexus test header "X-nexus-test".
        /// </summary>
        public string NexusTestContext
        {
            get => _nexusTestContext.GetValue();
            set => _nexusTestContext.SetValue(value);
        }

    }
}