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
            BeforeNext, CatchAfterNextAsync, FinallyAfterNextAsync, AfterNextAsync,
            CatchAfterMiddlewareAsync, FinallyAfterMiddlewareAsync
        };

        private ExpectedMethodEnum _latestMethod;
        protected readonly RequestDelegate Next;
        protected readonly NexusLinkMiddleWareOptions Options;

        /// <summary>
        /// This middleware is a collection of all the middleware features that are provided by Nexus Link. Use <paramref name="options"/>
        /// to specify exactly how they should behave.
        /// </summary>
        /// <param name="next">The inner handler</param>
        /// <param name="options">Options that controls which features to use and how they should behave.</param>
        public NexusLinkMiddleware(RequestDelegate next, NexusLinkMiddleWareOptions options)
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
        public NexusLinkMiddleware(RequestDelegate next, IOptions<NexusLinkMiddleWareOptions> options)
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
        /// This method has parts that you can override if you want to inherit from this class
        /// and add middleware: BeforeNext(), CatchAfterNextAsync(), FinallyAfterNextAsync(), AfterNextAsync(),
        /// CatchAfterMiddlewareAsync(), FinallyAfterMiddlewareAsync(). Always call the base method when you override,
        /// typically before your own functionality.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <remarks>
        /// The code looks like
        /// try {
        ///     stopWatch.Start();
        ///     await BeforeNext(context, stopWatch);
        ///     try {
        ///         await Next(context);
        ///         await AfterNextAsync(context, stopWatch);
        ///     } catch (Exception exception)
        ///         stopWatch.Stop();
        ///         var shouldThrow = await CatchAfterNextAsync(context, stopWatch, exception);
        ///         if (shouldThrow) throw;
        ///     } finally {
        ///         if (stopWatch.IsRunning) stopWatch.Stop();
        ///         await FinallyAfterNext(context, stopWatch);
        ///     }
        /// }
        /// catch (Exception exception) {
        ///     if (stopWatch.IsRunning) stopWatch.Stop();
        ///     var shouldThrow = await ExceptionAfterMiddlewareAsync(context, exception, stopWatch);
        ///     if (shouldThrow) throw;
        /// } finally {
        ///     await FinallyAfterMiddleware(context, stopWatch);
        /// }
        /// </remarks>
        public virtual async Task InvokeAsync(HttpContext context)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var nextSuccess = false;
            var middlewareSuccess = true;

            try
            {

                // Callback

                var callNext = BeforeNext(context, stopWatch);
                VerifyMethod(ExpectedMethodEnum.BeforeNext);

                // Callback

                try
                {
                    if (callNext)
                    {
                        await Next(context);
                        nextSuccess = true;
                    }
                    await AfterNextAsync(context, stopWatch, callNext);
                    VerifyMethod(ExpectedMethodEnum.AfterNextAsync);
                    stopWatch.Stop();
                }
                catch (Exception exception)
                {
                    stopWatch.Stop();
                    if (nextSuccess) middlewareSuccess = false;
                    var shouldThrow = await CatchAfterNextAsync(context, stopWatch, exception, nextSuccess, middlewareSuccess);

                    // Callback
                    
                    VerifyMethod(ExpectedMethodEnum.CatchAfterNextAsync);
                    if (shouldThrow) throw;
                }
                finally
                {
                    await FinallyAfterNextAsync(context, stopWatch, nextSuccess, middlewareSuccess);
                    VerifyMethod(ExpectedMethodEnum.FinallyAfterNextAsync);
                }
            }
            catch (Exception exception)
            {
                if (nextSuccess) middlewareSuccess = false;
                if (stopWatch.IsRunning) stopWatch.Stop();
                try
                {
                    var shouldThrow = await CatchAfterMiddlewareAsync(context, exception, stopWatch, nextSuccess,
                        middlewareSuccess);
                    VerifyMethod(ExpectedMethodEnum.CatchAfterMiddlewareAsync);
                    if (shouldThrow) throw;
                }
                catch (Exception)
                {
                    middlewareSuccess = false;
                    throw;
                }

            }
            finally
            {
                if (stopWatch.IsRunning) stopWatch.Stop();
                await FinallyAfterMiddlewareAsync(context, stopWatch, nextSuccess, middlewareSuccess);
                VerifyMethod(ExpectedMethodEnum.FinallyAfterMiddlewareAsync);
            }
        }

        private void VerifyMethod(ExpectedMethodEnum expectedMethod)
        {
            InternalContract.Require(expectedMethod == _latestMethod,
                $"Seems like you have overridden the method {expectedMethod}, but didn't call the base method, which is mandatory.");
        }

        /// <summary>
        /// Things that needs to be done before we actually call implementation method.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopWatch"> A stopwatch that was started when <see cref="InvokeAsync"/> started.</param>
        /// <returns>A bool that indicates if the <see cref="Next"/> method should be called or not. false means that it should NOT be called.</returns>
        /// <remarks>This method can't be made async, as it sets <see cref="AsyncLocal{T}"/> variables.</remarks>
        protected virtual bool BeforeNext(HttpContext context, Stopwatch stopWatch)
        {
            _latestMethod = ExpectedMethodEnum.BeforeNext;
            if (Options.SaveClientTenant.Enabled)
            {
                var tenant = GetClientTenantFromUrl(context);
                FulcrumApplication.Context.ClientTenant = tenant;
            }

            if (Options.SaveCorrelationId.Enabled)
            {
                var correlationId = GetOrCreateCorrelationId(context);
                FulcrumApplication.Context.CorrelationId = correlationId;
            }

            if (Options.SaveTenantConfiguration.Enabled)
            {
                var tenantConfiguration = GetTenantConfigurationAsync(FulcrumApplication.Context.ClientTenant, context).Result;
                FulcrumApplication.Context.LeverConfiguration = tenantConfiguration;
            }

            if (Options.SaveNexusTestContext.Enabled)
            {
                var testContext = GetNexusTestContextFromHeader(context);
                FulcrumApplication.Context.NexusTestContext = testContext;
            }

            if (Options.BatchLog.Enabled)
            {
                BatchLogger.StartBatch(Options.BatchLog.Threshold, Options.BatchLog.FlushAsLateAsPossible);
            }

            return true;
        }

        /// <summary>
        /// This method is called immediately after a successful call to <see cref="Next"/>. It is called even when no call to <see cref="Next"/>
        /// was done (due to a true return value from the <see cref="BeforeNext"/> method).  
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopWatch"> A stopwatch that was started when <see cref="InvokeAsync"/> started.</param>
        /// <param name="nextWasCalled">True if the <see cref="Next"/> method was called, false if it was not called.</param>
        /// <returns>A bool that indicates if the original exception should be rethrown.</returns>
        protected virtual async Task AfterNextAsync(HttpContext context, Stopwatch stopWatch, bool nextWasCalled)
        {
            _latestMethod = ExpectedMethodEnum.AfterNextAsync;
            if (Options.LogRequestAndResponse.Enabled)
            {
                await LogResponseAsync(context, stopWatch.Elapsed);
            }
        }

        /// <summary>
        /// This method is called if one of the <see cref="Next"/> and <see cref="AfterNextAsync"/> methods throws.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopWatch"> A stopwatch that was started when <see cref="InvokeAsync"/> started.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="nextSuccess">True if next succeeded.</param>
        /// <param name="middlewareSuccess">True if the middleware succeeded.</param>
        /// <returns>A bool that indicates if the original exception should be rethrown.</returns>
        /// <remarks>At this point only one of <paramref name="nextSuccess"/> and <paramref name="middlewareSuccess"/> can be true.</remarks>
        protected virtual async Task<bool> CatchAfterNextAsync(HttpContext context, Stopwatch stopWatch,
            Exception exception, bool nextSuccess, bool middlewareSuccess)
        {
            _latestMethod = ExpectedMethodEnum.CatchAfterNextAsync;
            var throwOriginalException = true;
            if (Options.ConvertExceptionToHttpResponse.Enabled)
            {
                await ConvertExceptionToResponseAsync(context, exception);
                throwOriginalException = false;
                
                if (Options.LogRequestAndResponse.Enabled)
                {
                    await LogResponseAsync(context, stopWatch.Elapsed);
                }
            }

            return throwOriginalException;
        }

        /// <summary>
        /// This method is called finally after the <see cref="Next"/> method, no matter if it succeeded or threw an exception.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopWatch"> A stopwatch that was started when <see cref="InvokeAsync"/> started.</param>
        /// <param name="nextSuccess">True if next succeeded.</param>
        /// <param name="middlewareSuccess">True if the middleware succeeded.</param>
        protected virtual Task FinallyAfterNextAsync(HttpContext context, Stopwatch stopWatch, bool nextSuccess,
            bool middlewareSuccess)
        {
            _latestMethod = ExpectedMethodEnum.FinallyAfterNextAsync;
            return Task.CompletedTask;
        }

        /// <summary>
        /// The main objective for this method is to be a catch all, for both exceptions from <see cref="Next"/> and from the middleware logic.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopWatch"> A stopwatch that was started when <see cref="InvokeAsync"/> started.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="nextSuccess">True if next succeeded.</param>
        /// <param name="middlewareSuccess">True if the middleware succeeded.</param>
        /// <returns>A bool that indicates if the original exception should be rethrown.</returns>
        protected virtual Task<bool> CatchAfterMiddlewareAsync(HttpContext context, Exception exception,
            Stopwatch stopWatch, bool nextSuccess, bool middlewareSuccess)
        {
            _latestMethod = ExpectedMethodEnum.CatchAfterMiddlewareAsync;
            if (Options.LogRequestAndResponse.Enabled)
            {
                LogException(context, exception, stopWatch.Elapsed);
            }

            return Task.FromResult(true);
        }

        /// <summary>
        /// This method is called finally after all other methods have been called, even if an exception was thrown.
        /// </summary>
        /// <param name="context">The information about the current HTTP request.</param>
        /// <param name="stopWatch"> A stopwatch that was started when <see cref="InvokeAsync"/> started.</param>
        /// <param name="nextSuccess">True if next succeeded.</param>
        /// <param name="middlewareSuccess">True if the middleware succeeded.</param>
        protected virtual Task FinallyAfterMiddlewareAsync(HttpContext context, Stopwatch stopWatch, bool nextSuccess,
            bool middlewareSuccess)
        {
            _latestMethod = ExpectedMethodEnum.FinallyAfterMiddlewareAsync;
            if (Options.BatchLog.Enabled) BatchLogger.EndBatch();
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
                var service = Options.SaveTenantConfiguration.ServiceConfiguration;
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

        protected Tenant GetClientTenantFromUrl(HttpContext context)
        {
            var match = Options.SaveClientTenant.RegexForFindingTenantInUrl.Match(context.Request.Path);
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
        public static IApplicationBuilder UseNexusLinkMiddleware(this IApplicationBuilder builder, IOptions<NexusLinkMiddleWareOptions> options)
        {
            return builder.UseMiddleware<NexusLinkMiddleware>(options);
        }
    }
}
#endif
