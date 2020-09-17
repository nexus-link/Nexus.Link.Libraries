
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
#if NETCOREAPP
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endif
namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    /// <summary>
    /// Extracts organization and environment values from request uri and adds these values to an execution context. 
    /// These values are later used to get organization and environment specific configurations for logging and request handling. 
    /// </summary>
    public class SaveClientTenant : CompatibilityDelegatingHandler
    {
        private static readonly DelegateState DelegateState = new DelegateState(typeof(SaveClientTenant).FullName);
        private readonly Regex _regex;

        /// <summary>
        /// True if this delegate has started in the current context
        /// </summary>
        public static bool HasStarted
        {
            get => DelegateState.HasStarted;
            private set => DelegateState.HasStarted = value;
        }

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

#if NETCOREAPP
        /// <inheritdoc />
        public SaveClientTenant(RequestDelegate next, string tenantPrefix)
            : base(next)
        {
            _regex = new Regex($"{tenantPrefix}/([^/]+)/([^/]+)/");
        }
#else
        /// <summary>
        /// Set up saving the client tenant.
        /// </summary>
        /// <param name="tenantPrefix">The prefix before the "/{organization}/{environment}/" part of the path. </param>
        /// <remarks><see cref="LegacyVersionPrefix"/>, <see cref="LegacyApiVersionPrefix"/>, <see cref="ApiVersionTenantPrefix"/></remarks>
        public SaveClientTenant(string tenantPrefix)
        {
            _regex = new Regex($"{tenantPrefix}/([^/]+)/([^/]+)/");
        }
#endif

        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            InternalContract.Require(!DelegateState.HasStarted, $"{nameof(SaveClientTenant)} has already been started in this http request.");
            HasStarted = true;
#if NETCOREAPP
            var match = _regex.Match(context.Context.Request.Path);
#else
                var match = _regex.Match(context.RequestMessage.RequestUri.PathAndQuery);
#endif
            if (match.Success && match.Groups.Count == 3)
            {
                var organization = match.Groups[1].Value;
                var environment = match.Groups[2].Value;

                var tenant = new Tenant(organization, environment);
                FulcrumApplication.Context.ClientTenant = tenant;
            }

            await CallNextDelegateAsync(context);
        }
    }
#if NETCOREAPP
    public static class SaveClientTenantExtension
    {
        public static IApplicationBuilder UseNexusSaveClientTenant(
            this IApplicationBuilder builder,
            string tenantPrefix)
        {
            return builder.UseMiddleware<SaveClientTenant>(tenantPrefix);
        }
    }
#endif
}
