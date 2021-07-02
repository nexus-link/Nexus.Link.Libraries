#if NETCOREAPP
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Logging;
using Nexus.Link.Libraries.Web.AspNet.Queue;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    /// <summary>
    /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <see name="INexusLinkMiddlewareOptions"/>
    /// to specify exactly how they should behave.
    /// </summary>
    public class NexusLinkMiddleware
    {
        protected readonly RequestDelegate Next;
        protected readonly NexusLinkMiddlewareOptions Options;

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="next">The inner handler</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public NexusLinkMiddleware(RequestDelegate next, NexusLinkMiddlewareOptions options)
        {
            InternalContract.RequireValidated(options, nameof(options));

            Next = next;
            Options = options;
        }

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="next">The inner handler</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public NexusLinkMiddleware(RequestDelegate next, IOptions<NexusLinkMiddlewareOptions> options)
        {
            InternalContract.RequireNotNull(options.Value, nameof(options));
            InternalContract.RequireValidated(options.Value, nameof(options));

            Next = next;
            Options = options.Value;
        }

        // TODO: Make code example complete
        // TODO: Make callbacks in options
        // TODO: Move code into one big invoke

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// The main method for a middleware.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var cancellationToken = context.RequestAborted;

            try
            {
                if (Options.Features.SaveClientTenant.Enabled)
                {
                    var tenant = GetClientTenantFromUrl(context);
                    FulcrumApplication.Context.ClientTenant = tenant;
                }

                if (Options.Features.SaveCorrelationId.Enabled)
                {
                    var correlationId = GetOrCreateCorrelationId(context);
                    FulcrumApplication.Context.CorrelationId = correlationId;
                }

                if (Options.Features.SaveTenantConfiguration.Enabled)
                {
                    var tenantConfiguration = await GetTenantConfigurationAsync(FulcrumApplication.Context.ClientTenant, context, cancellationToken);
                    FulcrumApplication.Context.LeverConfiguration = tenantConfiguration;
                }

                if (Options.Features.SaveNexusTestContext.Enabled)
                {
                    var testContext = GetNexusTestContextFromHeader(context);
                    FulcrumApplication.Context.NexusTestContext = testContext;
                }

                if (Options.Features.BatchLog.Enabled)
                {
                    BatchLogger.StartBatch(Options.Features.BatchLog.Threshold, Options.Features.BatchLog.FlushAsLateAsPossible);
                }

                try
                {
                    await Next(context);
                    if (Options.Features.LogRequestAndResponse.Enabled)
                    {
                        await LogResponseAsync(context, stopwatch.Elapsed, cancellationToken);
                    }
                }
                catch (Exception exception)
                {

                    var shouldThrow = true;
                    if (Options.Features.ConvertExceptionToHttpResponse.Enabled)
                    {
                        await ConvertExceptionToResponseAsync(context, exception, cancellationToken);
                        shouldThrow = false;
                        if (Options.Features.LogRequestAndResponse.Enabled)
                        {
                            await LogResponseAsync(context, stopwatch.Elapsed, cancellationToken);
                        }
                    }
                    if (shouldThrow) throw;
                }
            }
            catch (Exception exception)
            {
                if (Options.Features.LogRequestAndResponse.Enabled)
                {
                    LogException(context, exception, stopwatch.Elapsed);
                }
                throw;
            }
            finally
            {
                if (Options.Features.BatchLog.Enabled) BatchLogger.EndBatch();
            }
        }

        #region SaveNexusTestContextToContext
        /// <summary>
        /// 
        /// </summary>
        protected static string GetNexusTestContextFromHeader(HttpContext context)
        {
            var request = context?.Request;
            FulcrumAssert.IsNotNull(request, CodeLocation.AsString());
            if (request == null) return null;
            var headerValueExists = request.Headers.TryGetValue(Constants.NexusTestContextHeaderName, out var values);
            if (!headerValueExists) return null;
            var valuesAsArray = values.ToArray();
            if (!valuesAsArray.Any()) return null;
            if (valuesAsArray.Length == 1) return valuesAsArray[0];
            var message = $"There was more than one {Constants.NexusTestContextHeaderName} headers: {string.Join(", ", valuesAsArray)}. Using the first one.";
            Log.LogWarning(message);
            return valuesAsArray[0];
        }

        #endregion

        #region SaveCorrelationIdInContext
        protected string GetOrCreateCorrelationId(HttpContext context)
        {
            var request = context?.Request;
            var correlationId = ExtractCorrelationIdFromHeader(context);
            var createNewCorrelationId = string.IsNullOrWhiteSpace(correlationId);
            if (createNewCorrelationId) correlationId = Guid.NewGuid().ToString();
            if (createNewCorrelationId)
            {
                Log.LogInformation(
                    $"Created correlation id {correlationId}, as incoming request did not have it. ({request?.ToLogString()})");
            }

            return correlationId;
        }

        protected static string ExtractCorrelationIdFromHeader(HttpContext context)
        {
            var request = context?.Request;

            FulcrumAssert.IsNotNull(request, CodeLocation.AsString());
            if (request == null) return null;
            if (!request.Headers.TryGetValue(Constants.FulcrumCorrelationIdHeaderName, out var correlationIds))
            {
                return null;
            }
            var correlationsArray = correlationIds.ToArray();
            if (!correlationsArray.Any()) return null;
            if (correlationsArray.Length == 1) return correlationsArray[0];
            var message =
                $"There was more than one correlation id in the header: {string.Join(", ", correlationsArray)}. The first one was picked as the Fulcrum correlation id from here on.";
            Log.LogWarning(message);
            return correlationsArray[0];
        }
        #endregion

        #region GetTenantConfiguration
        protected async Task<ILeverConfiguration> GetTenantConfigurationAsync(Tenant tenant, HttpContext context,
            CancellationToken cancellationToken)
        {
            if (tenant == null) return null;
            try
            {
                var service = Options.Features.SaveTenantConfiguration.ServiceConfiguration;
                return await service.GetConfigurationForAsync(tenant, cancellationToken);
            }
            catch (FulcrumUnauthorizedException e)
            {
                throw new FulcrumResourceException($"Could not fetch configuration for Tenant: '{tenant}': {e.Message}", e);
            }
            catch
            {
                // Deliberately ignore errors for configuration. This will have to be taken care of when the configuration is needed.
                return null;
            }
        }
        #endregion

        #region SaveClientTenant

        protected Tenant GetClientTenantFromUrl(HttpContext context)
        {
            var match = Options.Features.SaveClientTenant.RegexForFindingTenantInUrl.Match(context.Request.Path);
            if (!match.Success || match.Groups.Count != 3) return null;
            var organization = match.Groups[1].Value;
            var environment = match.Groups[2].Value;

            var tenant = new Tenant(organization, environment);
            return tenant;

        }
        #endregion

        #region ConvertExceptionToHttpResponse
        protected static async Task ConvertExceptionToResponseAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            var response = AspNetExceptionConverter.ToContentResult(exception);
            FulcrumAssert.IsTrue(response.StatusCode.HasValue, CodeLocation.AsString());
            Debug.Assert(response.StatusCode.HasValue);
            context.Response.StatusCode = response.StatusCode.Value;
            context.Response.ContentType = response.ContentType;
            await context.Response.WriteAsync(response.Content, cancellationToken: cancellationToken);
        }
        #endregion

        #region LogRequestResponse
        protected static async Task LogResponseAsync(HttpContext context, TimeSpan elapsedTime, CancellationToken cancellationToken)
        {
            var logLevel = LogSeverityLevel.Information;
            var request = context.Request;
            var response = context.Response;
            if (response.StatusCode >= 500) logLevel = LogSeverityLevel.Error;
            else if (response.StatusCode >= 400) logLevel = LogSeverityLevel.Warning;
            Log.LogOnLevel(logLevel, $"INBOUND request-response {await request.ToLogStringAsync(response, elapsedTime, cancellationToken)}");
        }

        protected static void LogException(HttpContext context, Exception exception, TimeSpan elapsedTime)
        {
            var request = context.Request;
            Log.LogError($"INBOUND request-exception {request.ToLogString(elapsedTime)} | {exception.Message}", exception);
        }
        #endregion
    }

    /// <summary>
    /// Convenience class for middleware
    /// </summary>
    public static class NexusLinkMiddlewareExtension
    {
        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="builder">"this"</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public static IApplicationBuilder UseNexusLinkMiddleware(this IApplicationBuilder builder, IOptions<NexusLinkMiddlewareOptions> options)
        {
            return builder.UseMiddleware<NexusLinkMiddleware>(options);
        }

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="builder">"this"</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public static IApplicationBuilder UseNexusLinkMiddleware(this IApplicationBuilder builder, NexusLinkMiddlewareOptions options)
        {
            return builder.UseMiddleware<NexusLinkMiddleware>(options);
        }
    }
}
#endif
