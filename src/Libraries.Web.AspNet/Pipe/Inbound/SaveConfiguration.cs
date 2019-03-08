using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;

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
    [Obsolete("Use SaveClientTenant and SaveClientTenantConfiguration")]
    public class SaveConfiguration : CompatibilityDelegatingHandler
    {
        private readonly ILeverServiceConfiguration _serviceConfiguration;
        private static readonly DelegateState DelegateState = new DelegateState(typeof(SaveConfiguration).FullName);

        /// <summary>
        /// True if this delegate has started in the current context
        /// </summary>
        public static bool HasStarted
        {
            get => DelegateState.HasStarted;
            private set => DelegateState.HasStarted = value;
        }

#if NETCOREAPP
        /// <inheritdoc />
        public SaveConfiguration(RequestDelegate next, ILeverServiceConfiguration serviceConfiguration)
            : base(next)
        {
            _serviceConfiguration = serviceConfiguration;
        }
#else
        public SaveConfiguration(ILeverServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
        }
#endif

        protected override async Task InvokeAsync(CompabilityInvocationContext context)
        {
            HasStarted = true;
            var rgx = new Regex("/v[^/]+/([^/]+)/([^/]+)/");

#if NETCOREAPP
            var match = rgx.Match(context.Context.Request.Path);
#else
            var match = rgx.Match(context.RequestMessage.RequestUri.PathAndQuery);
#endif
            if (match.Success && match.Groups.Count == 3)
            {
                var organization = match.Groups[1].Value;
                var environment = match.Groups[2].Value;

                var tenant = new Tenant(organization, environment);
                FulcrumApplication.Context.ClientTenant = tenant;
                try
                {
                    // SaveConfiguration should be run before SaveCorrelationId, so setup correlation id from request header if necessary
                    if (string.IsNullOrWhiteSpace(FulcrumApplication.Context.CorrelationId))
                    {
                        var correlationId = SaveCorrelationId.ExtractCorrelationIdFromHeader(context);
                        if (!string.IsNullOrWhiteSpace(correlationId))
                            FulcrumApplication.Context.CorrelationId = correlationId;
                    }

                    FulcrumApplication.Context.LeverConfiguration =
                        await _serviceConfiguration.GetConfigurationForAsync(tenant);
                }
                catch
                {
                    // Deliberately ignore errors for configuration. This will have to be taken care of when the configuration is needed.
                }
            }

            await CallNextDelegateAsync(context);
        }
    }
#if NETCOREAPP
    public static class SaveConfigurationExtension
    {
        [Obsolete("Use UseNexusSaveClientTenant and UseNexusSaveClientConfigurationTenant")]
        public static IApplicationBuilder UseNexusSaveConfiguration(
            this IApplicationBuilder builder, ILeverServiceConfiguration serviceConfiguration)
        {
            return builder.UseMiddleware<SaveConfiguration>(serviceConfiguration);
        }
    }
#endif
}
