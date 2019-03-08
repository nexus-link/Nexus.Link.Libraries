using System;
using Nexus.Link.Libraries.Core.Application;

namespace Nexus.Link.Libraries.Core.Context
{
    /// <summary>
    /// Stores CorrelationId in the execution context.
    /// </summary>
    [Obsolete("Use FulcrumApplication.Context.CallingClientName.", true)]
    public class CallingClientValueProvider
    {
        private static bool _firstTime = true;
        private readonly OneValueProvider<string> _valueProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        public CallingClientValueProvider()
        {
            if (_firstTime)
            {
                FulcrumApplication.Validate();
                _firstTime = false;
            }
            _valueProvider = new OneValueProvider<string>(FulcrumApplication.Context.ValueProvider, "CallingClientName");
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public string CallingClientName
        {
            get => _valueProvider.GetValue();
            set => _valueProvider.SetValue(value);
        }
    }
}
