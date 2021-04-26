using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Libraries.Core.Platform.Configurations
{
    /// <summary>
    /// Interface for retrieving configuration data.
    /// </summary>
    public interface ILeverConfiguration
    {
        /// <summary>
        /// The <see cref="Tenant"/> for this configuration.
        /// </summary>
        Tenant Tenant { get; }

        /// <summary>
        /// Gets a value from the configuration and verifies that it is not null.
        /// </summary>
        T MandatoryValue<T>(object key);

        /// <summary>
        /// Gets a value from the configuration.
        /// </summary>
        T Value<T>(object key);
    }
}