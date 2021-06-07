namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Options
{
    /// <summary>
    /// The prefix before the "/{organization}/{environment}/" part of the path. This is used to pattern match where we would find the organization and environment.
    /// Here are some common patterns: <see cref="LegacyVersionPrefix"/>, <see cref="LegacyApiVersionPrefix"/>,
    /// <see cref="ApiVersionTenantPrefix"/>
    /// </summary>
    public class SaveClientTenantOptions : Feature
    {
        /// <summary>
        /// The way that many XLENT Link services has prefixed tenants in their path. Not recommended. <see cref="ApiVersionTenantPrefix"/> for the recommended prefix.
        /// </summary>
        public const string LegacyVersionPrefix = "/v[^/]+";

        /// <summary>
        /// A slightly safer way than <see cref="LegacyVersionPrefix"/>. Not recommended. <see cref="ApiVersionTenantPrefix"/> for the recommended prefix.
        /// </summary>
        public const string LegacyApiVersionPrefix = "api/v[^/]+";

        /// <summary>
        /// The current recommended prefix for tenant in path
        /// </summary>
        public const string ApiVersionTenantPrefix = "api/v[^/]+/Tenant";
    }
}