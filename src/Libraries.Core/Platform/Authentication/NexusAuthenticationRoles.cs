namespace Nexus.Link.Libraries.Core.Platform.Authentication
{
    /// <summary>
    /// Describes roles in the Nexus' tenants database, used for accessing the Nexus services.
    /// </summary>
    public static class NexusAuthenticationRoles
    {
        /// <summary>
        /// Represents one of the Nexus services itself.
        /// </summary>
        public const string PlatformService = "platform-service";

        /// <summary>
        /// Represents a customer's api client
        /// </summary>
        public const string Api = "api";
    }
}
