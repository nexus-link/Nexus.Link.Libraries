using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
#if NETCOREAPP
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Error.Logic;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Core.Assert;
#else
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound
{
    /// <summary>
    /// If the call results in an exception, this handler converts the exception into a response with the corresponding <see cref="FulcrumError"/>.
    /// </summary>
#if NETCOREAPP
    [Obsolete("Use ExceptionToFulcrumResponse. Obsolete warning since 2020-06-09, error since 2021-06-09.", true)]
    public class ConvertExceptionToFulcrumResponse
    {
        private readonly RequestDelegate _next;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ConvertExceptionToFulcrumResponse(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (FulcrumApplication.Context.RequestStopwatch == null)
                {
                    FulcrumApplication.Context.RequestStopwatch = new Stopwatch();
                    FulcrumApplication.Context.RequestStopwatch.Start();
                }
                await _next(context);
            }
            catch (Exception exception)
            {
                Log.LogError($"The web service had an internal exception ({exception.Message})", exception);

                var response = AspNetExceptionConverter.ToContentResult(exception);
                Log.LogInformation(
                    $"Exception ({exception.Message}) was converted to an HTTP response ({response.StatusCode}).");

                FulcrumAssert.IsTrue(response.StatusCode.HasValue, CodeLocation.AsString());
                Debug.Assert(response.StatusCode.HasValue);
                context.Response.StatusCode = response.StatusCode.Value;
                context.Response.ContentType = response.ContentType;
                await context.Response.WriteAsync(response.Content);

                if (exception is FulcrumHttpRedirectException redirectException)
                {
                    context.Response.Headers.Add("Location", redirectException.LocationUri.OriginalString);
                }
            }
        }
    }

    [Obsolete("Use ExceptionToFulcrumResponseExtension. Obsolete warning since 2020-06-09, error since 2021-06-09.", true)]
    public static class ConvertExceptionToFulcrumResponseExtension
    {
        [Obsolete("Use UseNexusExceptionToFulcrumResponse. Obsolete warning since 2020-06-09, error since 2021-06-09.", true)]
        public static IApplicationBuilder UseNexusConvertExceptionToFulcrumResponse(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConvertExceptionToFulcrumResponse>();
        }
    }
#else
    public class ConvertExceptionToFulcrumResponse : ExceptionHandler
    {
        /// <summary>Converts the exception into a response with the corresponding <see cref="FulcrumError"/>.</summary>
        /// <returns>A task representing the asynchronous exception handling operation.</returns>
        /// <param name="context">The exception handler context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public override async Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            if (context?.Exception != null)
            {
                Log.LogError($"The web service had an internal exception ({context.Exception.Message})", context.Exception);

                var response = AspNetExceptionConverter.ToHttpResponseMessage(context.Exception, cancellationToken);
                Log.LogInformation($"Exception ({context.Exception.Message}) was converted to an HTTP response ({response.StatusCode}).");

                context.Result = new ErrorResult(context.Request, response, FulcrumApplication.Context.CorrelationId);
            }

            await base.HandleAsync(context, cancellationToken);
        }
    #region Private Helpers

        private class ErrorResult : IHttpActionResult
        {
            private readonly HttpResponseMessage _response;

            public ErrorResult(HttpRequestMessage request, HttpResponseMessage response, string correlationId)
            {
                _response = response;
                _response.Headers.Add("X-Correlation-ID", new List<string> { correlationId });
                _response.RequestMessage = request;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    #endregion
    }
#endif
}