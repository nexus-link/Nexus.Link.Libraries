using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
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
    public class SaveClientTenantConfiguration : CompatibilityDelegatingHandlerWithCancellationSupport
    {
        private readonly ILeverServiceConfiguration _serviceConfiguration;
        private static readonly DelegateState DelegateState = new DelegateState(typeof(SaveClientTenantConfiguration).FullName);

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
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public SaveClientTenantConfiguration(RequestDelegate next, ILeverServiceConfiguration serviceConfiguration)
                : base(next)
        {
            _serviceConfiguration = serviceConfiguration;
        }
#else
        public SaveClientTenantConfiguration(ILeverServiceConfiguration serviceConfiguration)
        {
            _serviceConfiguration = serviceConfiguration;
        }
#endif

        protected override async Task InvokeAsync(CompabilityInvocationContext context, CancellationToken cancellationToken)
        {
            InternalContract.Require(!DelegateState.HasStarted, $"{nameof(SaveClientTenantConfiguration)} has already been started in this http request.");
            InternalContract.Require(SaveClientTenant.HasStarted, $"{nameof(SaveClientTenantConfiguration)} must be preceded by {nameof(SaveClientTenant)}.");
            InternalContract.Require(!SaveCorrelationId.HasStarted, $"{nameof(SaveCorrelationId)} must not precede {nameof(SaveClientTenantConfiguration)}");

            HasStarted = true;
            var tenant = FulcrumApplication.Context.ClientTenant;
            if (tenant != null)
            {
                // SaveConfiguration should be run before SaveCorrelationId, so setup correlation id from request header if necessary
                if (string.IsNullOrWhiteSpace(FulcrumApplication.Context.CorrelationId))
                {
                    var correlationId = SaveCorrelationId.ExtractCorrelationIdFromHeader(context);
                    if (!string.IsNullOrWhiteSpace(correlationId)) FulcrumApplication.Context.CorrelationId = correlationId;
                }

                // TODO: timer?
                for (var i = 0; i < 3; i++)
                {
                    try
                    {
                        FulcrumApplication.Context.LeverConfiguration = await _serviceConfiguration.GetConfigurationForAsync(tenant, cancellationToken);
                        break;
                    }
                    catch (FulcrumTryAgainException)
                    {
                        await TaskHelper.RandomDelayAsync(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(0.5), cancellationToken);
                        continue;
                    }
                    catch (FulcrumNotFoundException e)
                    {
                        throw new FulcrumNotFoundException($"{FulcrumApplication.Setup.Name} could not find its configuration in Fundamentals for tenant {tenant}: {e}", e);
                    }
                    catch (Exception e)
                    {
                        if (cancellationToken.IsCancellationRequested) // AND 120 s
                        {
                            // TODO: Another exception?
                        }
                        throw new FulcrumAssertionFailedException($"{FulcrumApplication.Setup.Name} could not get its configuration in Fundamentals for tenant {tenant}: {e}", e); ;
                    }
                }
            }

            await CallNextDelegateAsync(context, cancellationToken);
        }
    }
#if NETCOREAPP
    public static class SaveClientTenantConfigurationExtension
    {
        [Obsolete("Please use the class NexusLinkMiddleware. Obsolete since 2021-06-04")]
        public static IApplicationBuilder UseNexusSaveClientTenantConfiguration(
            this IApplicationBuilder builder, ILeverServiceConfiguration serviceConfiguration)
        {
            return builder.UseMiddleware<SaveClientTenantConfiguration>(serviceConfiguration);
        }
    }
#endif
}
