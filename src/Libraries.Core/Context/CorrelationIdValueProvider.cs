using System;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Libraries.Core.Context
{
    /// <summary>
    /// Stores CorrelationId in the execution context.
    /// </summary>
    [Obsolete("Use FulcrumApplication.Context.CorrelationId.", true)]
    public class CorrelationIdValueProvider
    {
        private static bool _firstTime = true;
        private readonly OneValueProvider<string> _valueProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        public CorrelationIdValueProvider()
        {
            if (_firstTime)
            {
                FulcrumApplication.Validate();
                _firstTime = false;
            }
            _valueProvider = new OneValueProvider<string>(FulcrumApplication.Context.ValueProvider, "NexusCorrelationId");
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public string CorrelationId
        {
            get => _valueProvider.GetValue();
            set => _valueProvider.SetValue(value);
        }
    }
}
