using System;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Context;

namespace Nexus.Link.Libraries.Core.Platform.Configurations
{
    /// <summary>
    /// Stores CorrelationId in the execution context.
    /// </summary>
    [Obsolete("Use FulcrumApplication.Context.LeverConfiguration.", true)]
    public class ConfigurationValueProvider
    {
        private static bool _firstTime = true;
        private readonly OneValueProvider<ILeverConfiguration> _valueProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConfigurationValueProvider()
        {
            if (_firstTime)
            {
                FulcrumApplication.Validate();
                _firstTime = false;
            }
            _valueProvider = new OneValueProvider<ILeverConfiguration>(FulcrumApplication.Context.ValueProvider, "LeverConfigurationId");
        }

        /// <summary>
        /// Access the context data
        /// </summary>
        public ILeverConfiguration LeverConfiguration
        {
            get => _valueProvider.GetValue();
            set => _valueProvider.SetValue(value);
        }
    }
}
