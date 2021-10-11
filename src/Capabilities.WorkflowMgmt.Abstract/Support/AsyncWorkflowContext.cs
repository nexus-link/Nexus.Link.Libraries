using System;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Support
{
    public class AsyncWorkflowContext
    {
        public IContextValueProvider ValueProvider { get; }

        private readonly OneValueProvider<bool> _executionIsAsynchronous;
        private readonly OneValueProvider<string> _workflowInstanceId;

        public AsyncWorkflowContext(IContextValueProvider valueProvider)
        {
            ValueProvider = valueProvider;
            _executionIsAsynchronous = new OneValueProvider<bool>(ValueProvider, "ExecutionIsAsynchronous");
            _workflowInstanceId = new OneValueProvider<string>(ValueProvider, "WorkflowInstanceId");
        }

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
        /// If non-null, contains the information about the current execution id.
        /// </summary>
        public string WorkflowInstanceId
        {
            get => _workflowInstanceId.GetValue();
            set => _workflowInstanceId.SetValue(value);
        }
    }
}