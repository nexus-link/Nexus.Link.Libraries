using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support
{
    public class AsyncWorkflowContext
    {
        public IContextValueProvider ValueProvider { get; }

        private readonly OneValueProvider<bool> _executionIsAsynchronous;
        private readonly OneValueProvider<AsyncExecutionContext> _asyncExecutionContext;

        public AsyncWorkflowContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _executionIsAsynchronous = new OneValueProvider<bool>(ValueProvider, "ExecutionIsAsynchronous");
            _asyncExecutionContext = new OneValueProvider<AsyncExecutionContext>(ValueProvider, "AsyncExecutionContext");
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
        public AsyncExecutionContext AsyncExecutionContext
        {
            get => _asyncExecutionContext.GetValue();
            set => _asyncExecutionContext.SetValue(value);
        }
    }
}