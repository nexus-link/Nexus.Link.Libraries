#if NETCOREAPP
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
using Nexus.Link.Libraries.Web.AspNet.Logging;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Nexus.Link.Libraries.Web.Pipe;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    /// <summary>
    /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <see name="INexusLinkMiddleWareOptions"/>
    /// to specify exactly how they should behave.
    /// </summary>
    public class NexusLinkMiddleware
    {
        private enum ExpectedMethodEnum
        {
            BeforeNextAsync,
            CatchAfterNextAsync, FinallyAfterNext, AfterNextAsync,
            CatchAfterMiddlewareAsync, FinallyAfterMiddleware
        };

        private ExpectedMethodEnum _latestMethod;
        protected readonly RequestDelegate Next;
        protected readonly INexusLinkMiddleWareOptions Options;

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="next">The inner handler</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public NexusLinkMiddleware(RequestDelegate next, INexusLinkMiddleWareOptions options)
        {
            InternalContract.RequireValidated(options, nameof(options));

            Next = next;
            Options = options;
            if (options.UseFeatureSaveClientTenantToContext)
            {
                Regex = new Regex($"{options.SaveClientTenantPrefix}/([^/]+)/([^/]+)/");
            }
        }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// This method has parts that you can override if you want to inherit from this class
        /// and add middleware: BeforeNextAsync(), CatchAfterNextAsync(), FinallyAfterNext(), AfterNextAsync(),
        /// CatchAfterMiddlewareAsync(), FinallyAfterMiddleware(). Always call the base method, or you will lose functionality in the current features.
        /// The code looks like
        /// try {
        ///     stopWatch.Start();
        ///     await BeforeNextAsync(context, stopWatch);
        ///     try {
        ///         await Next(context);
        ///     } catch (Exception exception)
        ///         stopWatch.Stop();
        ///         var shouldThrow = await CatchAfterNextAsync(context, stopWatch, exception);
        ///         if (shouldThrow) throw;
        ///     } finally {
        ///         if (stopWatch.IsRunning) stopWatch.Stop();
        ///         await FinallyAfterNext(context, stopWatch);
        ///     }
        ///     await AfterNextAsync(context, stopWatch);
        /// }
        /// catch (Exception exception) {
        ///     if (stopWatch.IsRunning) stopWatch.Stop();
        ///     var shouldThrow = await ExceptionAfterMiddlewareAsync(context, exception, stopWatch);
        ///     if (shouldThrow) throw;
        /// } finally {
        ///     await FinallyAfterMiddleware(context, stopWatch);
        /// }
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                await BeforeNextAsync(context, stopWatch);
                VerifyMethod(ExpectedMethodEnum.BeforeNextAsync);

                try
                {
                    await Next(context);
                }
                catch (Exception exception)
                {
                    stopWatch.Stop();
                    var shouldThrow = await CatchAfterNextAsync(context, stopWatch, exception);
                    VerifyMethod(ExpectedMethodEnum.CatchAfterNextAsync);
                    if (shouldThrow) throw;
                }
                finally
                {
                    if (stopWatch.IsRunning) stopWatch.Stop();
                    await FinallyAfterNext(context, stopWatch);
                    VerifyMethod(ExpectedMethodEnum.FinallyAfterNext);
                }
                await AfterNextAsync(context, stopWatch);
                VerifyMethod(ExpectedMethodEnum.AfterNextAsync);
            }
            catch (Exception exception)
            {
                if (stopWatch.IsRunning) stopWatch.Stop();
                var shouldThrow = await CatchAfterMiddlewareAsync(context, exception, stopWatch);
                VerifyMethod(ExpectedMethodEnum.CatchAfterMiddlewareAsync);
                if (shouldThrow) throw;
            }
            finally
            {
                await FinallyAfterMiddleware(context, stopWatch);
                VerifyMethod(ExpectedMethodEnum.FinallyAfterMiddleware);
            }
        }

        private void VerifyMethod(ExpectedMethodEnum expectedMethod)
        {
            InternalContract.Require(expectedMethod == _latestMethod,
                $"Seems like you have overridden the method {expectedMethod}, but didn't call the base method, which is mandatory.");
        }

        protected virtual async Task BeforeNextAsync(HttpContext context, Stopwatch stopWatch)
        {
            _latestMethod = ExpectedMethodEnum.BeforeNextAsync;
            if (Options.UseFeatureSaveClientTenantToContext && Regex != null)
            {
                var tenant = GetClientTenantFromUrl(context);
                FulcrumApplication.Context.ClientTenant = tenant;
            }

            if (Options.UseFeatureSaveCorrelationIdToContext)
            {
                var correlationId = GetOrCreateCorrelationId(context);
                FulcrumApplication.Context.CorrelationId = correlationId;
            }

            if (Options.UseFeatureSaveTenantConfigurationToContext)
            {
                var tenantConfiguration = await GetTenantConfigurationAsync(FulcrumApplication.Context.ClientTenant, context);
                FulcrumApplication.Context.LeverConfiguration = tenantConfiguration;
            }

            if (Options.UseFeatureSaveNexusTestContextToContext)
            {
                var testContext = GetNexusTestContextFromHeader(context);
                FulcrumApplication.Context.NexusTestContext = testContext;
            }

            if (Options.UseFeatureBatchLog)
            {
                BatchLogger.StartBatch(Options.BatchLogThreshold, Options.BatchLogReleaseRecordsAsLateAsPossible);
            }
        }

        protected virtual async Task<bool> CatchAfterNextAsync(HttpContext context, Stopwatch stopWatch, Exception exception)
        {
            _latestMethod = ExpectedMethodEnum.CatchAfterNextAsync;
            var throwOriginalException = true;
            if (Options.UseFeatureConvertExceptionToHttpResponse)
            {
                await ConvertExceptionToResponseAsync(context, exception);
                throwOriginalException = false;
            }

            return throwOriginalException;
        }

        protected virtual Task FinallyAfterNext(HttpContext context, Stopwatch stopWatch)
        {
            _latestMethod = ExpectedMethodEnum.FinallyAfterNext;
            return Task.CompletedTask;
        }

        protected virtual async Task AfterNextAsync(HttpContext context, Stopwatch stopWatch)
        {
            _latestMethod = ExpectedMethodEnum.AfterNextAsync;
            if (Options.UseFeatureLogRequestAndResponse)
            {
                await LogResponseAsync(context, stopWatch.Elapsed);
            }
        }

        protected virtual Task<bool> CatchAfterMiddlewareAsync(HttpContext context, Exception exception, Stopwatch stopWatch)
        {
            _latestMethod = ExpectedMethodEnum.CatchAfterMiddlewareAsync;
            if (Options.UseFeatureLogRequestAndResponse)
            {
                LogException(context, exception, stopWatch.Elapsed);
            }

            return Task.FromResult(true);
        }

        protected virtual Task FinallyAfterMiddleware(HttpContext context, Stopwatch stopWatch)
        {
            _latestMethod = ExpectedMethodEnum.FinallyAfterMiddleware;
            if (Options.UseFeatureBatchLog) BatchLogger.EndBatch();
            return Task.CompletedTask;
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
        protected async Task<ILeverConfiguration> GetTenantConfigurationAsync(Tenant tenant, HttpContext context)
        {
            if (tenant == null) return null;
            try
            {
                var service = Options.SaveTenantConfigurationServiceConfiguration;
                return await service.GetConfigurationForAsync(tenant);
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
        /// <summary>
        ///  The regular expression that is used to extract the organization and environment from the URL.
        /// </summary>
        protected readonly Regex Regex;

        protected Tenant GetClientTenantFromUrl(HttpContext context)
        {
            var match = Regex.Match(context.Request.Path);
            if (!match.Success || match.Groups.Count != 3) return null;
            var organization = match.Groups[1].Value;
            var environment = match.Groups[2].Value;

            var tenant = new Tenant(organization, environment);
            return tenant;

        }
        #endregion

        #region ConvertExceptionToHttpResponse
        protected static async Task ConvertExceptionToResponseAsync(HttpContext context, Exception exception)
        {
            var response = AspNetExceptionConverter.ToContentResult(exception);
            FulcrumAssert.IsTrue(response.StatusCode.HasValue, CodeLocation.AsString());
            Debug.Assert(response.StatusCode.HasValue);
            context.Response.StatusCode = response.StatusCode.Value;
            context.Response.ContentType = response.ContentType;
            await context.Response.WriteAsync(response.Content);
        }
        #endregion

        #region LogRequestResponse
        protected static async Task LogResponseAsync(HttpContext context, TimeSpan elapsedTime)
        {
            var logLevel = LogSeverityLevel.Information;
            var request = context.Request;
            var response = context.Response;
            if (response.StatusCode >= 500) logLevel = LogSeverityLevel.Error;
            else if (response.StatusCode >= 400) logLevel = LogSeverityLevel.Warning;
            Log.LogOnLevel(logLevel, $"INBOUND request-response {await request.ToLogStringAsync(response, elapsedTime)}");
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
        public static IApplicationBuilder UseNexusLinkMiddleware(this IApplicationBuilder builder, INexusLinkMiddleWareOptions options)
        {
            return builder.UseMiddleware<NexusLinkMiddleware>(options);
        }
    }
}
#endif
