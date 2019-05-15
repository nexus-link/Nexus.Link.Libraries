using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Logging;

namespace Nexus.Link.Libraries.Web.Pipe.Outbound
{
    /// <summary>
    /// Any non-successful response will be thrown as a <see cref="FulcrumException"/>.
    /// </summary>
    public class ThrowFulcrumExceptionOnFail : DelegatingHandler
    {

        /// <summary>
        /// A delegate for testing purposes
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public delegate Task<HttpResponseMessage> SendAsyncDelegate(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Use this in unit tests to inject code that will be called to simulate SendAsync().
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public SendAsyncDelegate UnitTest_SendAsyncDependencyInjection { get; set; }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestDescription = $"OUT request {request.ToLogString()}";
            HttpResponseMessage response;
            FulcrumException fulcrumException;
            try
            {
                if (UnitTest_SendAsyncDependencyInjection == null || !FulcrumApplication.IsInDevelopment)
                {
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    // This is for unit testing
                    response = await UnitTest_SendAsyncDependencyInjection(request, cancellationToken);
                }

                requestDescription = $"OUT request-response {await request.ToLogStringAsync(response)}";

                fulcrumException = await ExceptionConverter.ToFulcrumExceptionAsync(response);
                if (fulcrumException == null) return response;
            }
            catch (FulcrumException e)
            {
                Log.LogError($"{requestDescription} threw the exception {e.GetType().Name}: {e.TechnicalMessage}", e);
                throw;
            }
            catch (TaskCanceledException e)
            {
                var message = $"{requestDescription} was cancelled.";
                Log.LogWarning(message, e);
                throw new FulcrumTryAgainException(message, e);
            }
            catch (Exception e)
            {
                // If we end up here, we probably need to add another catch statement for that exception type.
                var message =
                    $"{requestDescription} failed with an exception of type {e.GetType().FullName}. Please report that the outbound pipe handler {nameof(ThrowFulcrumExceptionOnFail)} should catch this exception type explicitly.";
                Log.LogError(message, e);
                throw new FulcrumAssertionFailedException(message, e);
            }

            var severityLevel = (int) response.StatusCode > 500 ? LogSeverityLevel.Warning : LogSeverityLevel.Error;
            Log.LogOnLevel(severityLevel, $"{requestDescription} was converted to (and threw) the exception {fulcrumException.GetType().Name}: {fulcrumException.TechnicalMessage}", fulcrumException);
            throw fulcrumException;
        }
    }
}
