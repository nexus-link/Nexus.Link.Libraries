namespace Nexus.Link.Libraries.Core.Platform.Authentication
{
    /// <summary>
    /// Constants for claim types.
    /// </summary>
    public static class ClaimTypeNames
    {
        /// <summary>
        /// The name for the claim type Organization for the Nexus tenant
        /// </summary>
        public static string Organization => "nexus-tenant-org";
        /// <summary>
        /// The name for the claim type Environment for the Nexus tenant
        /// </summary>
        public static string Environment => "nexus-tenant-env";

        /// <summary>
        /// The LEGACY name for the claim type Organization for the Nexus tenant
        /// </summary>
        public static string LegacyOrganization => "org";
        /// <summary>
        /// The LEGACY name for the claim type Environment for the Nexus tenant
        /// </summary>
        public static string LegacyEnvironment => "env";
    }
}
