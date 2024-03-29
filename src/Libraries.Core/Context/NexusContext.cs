﻿using System;
using System.Diagnostics;
using System.Security.Principal;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

namespace Nexus.Link.Libraries.Core.Context
{
    public class NexusContext
    {
        public IContextValueProvider ValueProvider { get; }
        
        private readonly OneValueProvider<string> _correlationId;
        private readonly OneValueProvider<string> _parentExecutionId;
        private readonly OneValueProvider<string> _executionId;
        private readonly OneValueProvider<string> _childExecutionId;
        private readonly OneValueProvider<string> _childRequestDescription;
        private readonly OneValueProvider<bool> _executionIsAsynchronous;
        private readonly OneValueProvider<string> _asyncRequestId;
        private readonly OneValueProvider<double> _asyncPriority;
        private readonly OneValueProvider<string> _callingClientName;
        private readonly OneValueProvider<Tenant> _clientTenant;
        private readonly OneValueProvider<ILeverConfiguration> _leverConfiguration;
        private readonly OneValueProvider<IPrincipal> _clientPrincipal;
        private readonly OneValueProvider<IPrincipal> _userPrincipal;
        private readonly OneValueProvider<bool> _isInBatchLogger;
        private readonly OneValueProvider<string> _nexusTestContext;
        private readonly OneValueProvider<string> _managedAsynchronousRequestId;
        private readonly OneValueProvider<string> _reentryAuthentication;
        private readonly OneValueProvider<Stopwatch> _requestStopwatch;

        public NexusContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _correlationId = new OneValueProvider<string>(ValueProvider, "NexusCorrelationId");
            _parentExecutionId = new OneValueProvider<string>(ValueProvider, "NexusParentExecutionId");
            _executionId = new OneValueProvider<string>(ValueProvider, "NexusExecutionId");
            _childExecutionId = new OneValueProvider<string>(ValueProvider, "NexusChildExecutionId");
            _childRequestDescription = new OneValueProvider<string>(ValueProvider, "NexusChildExecutionDescription");
            _callingClientName = new OneValueProvider<string>(ValueProvider, "CallingClientName");
            _clientTenant = new OneValueProvider<Tenant>(ValueProvider, "TenantId");
            _leverConfiguration = new OneValueProvider<ILeverConfiguration>(ValueProvider, "LeverConfigurationId");
            _clientPrincipal = new OneValueProvider<IPrincipal>(ValueProvider, "ClientPrincipal");
            _userPrincipal = new OneValueProvider<IPrincipal>(ValueProvider, "UserPrincipal");
            _isInBatchLogger = new OneValueProvider<bool>(ValueProvider, "IsInBatchLogger");
            _nexusTestContext = new OneValueProvider<string>(ValueProvider, "NexusTestContext");
            _managedAsynchronousRequestId = new OneValueProvider<string>(ValueProvider, "ManagedAsynchronousRequestId");
            _reentryAuthentication = new OneValueProvider<string>(ValueProvider, "ReentryAuthentication");
            _executionIsAsynchronous = new OneValueProvider<bool>(ValueProvider, "ExecutionIsAsynchronous");
            _asyncRequestId = new OneValueProvider<string>(ValueProvider, "AsyncRequestId");
            _asyncPriority = new OneValueProvider<double>(ValueProvider, "AsyncPriority");
            _requestStopwatch = new OneValueProvider<Stopwatch>(ValueProvider, "RequestStopwatch");
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public Guid ContextId => ValueProvider.ContextId;

        /// <summary>
        /// The correlation id.
        /// </summary>
        public string CorrelationId
        {
            get => _correlationId.GetValue();
            set => _correlationId.SetValue(value);
        }

        /// <summary>
        /// The parent execution id.
        /// </summary>
        public string ParentExecutionId
        {
            get => _parentExecutionId.GetValue();
            set => _parentExecutionId.SetValue(value);
        }

        /// <summary>
        /// The execution id.
        /// </summary>
        public string ExecutionId
        {
            get => _executionId.GetValue();
            set => _executionId.SetValue(value);
        }

        /// <summary>
        /// The child execution id.
        /// </summary>
        public string ChildExecutionId
        {
            get => _childExecutionId.GetValue();
            set => _childExecutionId.SetValue(value);
        }

        /// <summary>
        /// The child execution description.
        /// </summary>
        public string ChildRequestDescription
        {
            get => _childRequestDescription.GetValue();
            set => _childRequestDescription.SetValue(value);
        }

        /// <summary>
        /// True if the current execution is running as an asynchronous execution.
        /// </summary>
        public bool ExecutionIsAsynchronous
        {
            get => _executionIsAsynchronous.GetValue();
            set => _executionIsAsynchronous.SetValue(value);
        }

        /// <summary>
        /// If the current outbound request has been sent to the asynchronous request management capability, then this is the RequestId for that request.
        /// </summary>
        public string AsyncRequestId
        {
            get => _asyncRequestId.GetValue();
            set => _asyncRequestId.SetValue(value);
        }

        /// <summary>
        /// If <see cref="ExecutionIsAsynchronous"/> is set, this is the priority for this execution.
        /// </summary>
        public double AsyncPriority
        {
            get => _asyncPriority.GetValue();
            set => _asyncPriority.SetValue(value);
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

        /// <summary>
        /// If non-null, the current request is managed by an async management capailiby.
        /// </summary>
        public string ManagedAsynchronousRequestId
        {
            get => _managedAsynchronousRequestId.GetValue();
            set => _managedAsynchronousRequestId.SetValue(value);
        }

        /// <summary>
        /// If non-null, we have saved a reentry token key, which enables us to use expired tokens on reentry.
        /// </summary>
        public string ReentryAuthentication
        {
            get => _reentryAuthentication.GetValue();
            set => _reentryAuthentication.SetValue(value);
        }

        /// <summary>
        /// If non-null, a stopwatch that was started at the beginning of executing an inbound request
        /// </summary>
        /// <remarks>
        /// This is used in a WEB API environment.
        /// </remarks>
        public Stopwatch RequestStopwatch
        {
            get => _requestStopwatch.GetValue();
            set => _requestStopwatch.SetValue(value);
        }

    }
}