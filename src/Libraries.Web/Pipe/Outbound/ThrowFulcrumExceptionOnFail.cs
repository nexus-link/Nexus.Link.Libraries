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
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response;
                if (UnitTest_SendAsyncDependencyInjection == null)
                {
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    // This is for unit testing
                    FulcrumAssert.IsTrue(FulcrumApplication.IsInDevelopment);
                    response = await UnitTest_SendAsyncDependencyInjection(request, cancellationToken);
                }

                var fulcrumException = await ExceptionConverter.ToFulcrumExceptionAsync(response);
                if (fulcrumException == null) return response;
                Log.LogInformation(
                    $"OUT request-response {response.ToLogString()} was converted to a FulcrumException:\r{fulcrumException.ToLogString()}");
                throw fulcrumException;
            }
            catch (FulcrumException)
            {
                throw;
            }
            catch (TaskCanceledException e)
            {
                var message = $"HTTP request {request.ToLogString()} was cancelled.";
                Log.LogWarning(message, e);
                throw new FulcrumTryAgainException(message, e);
            }
            catch (Exception e)
            {
                // If we end up here, we probably need to add another catch statement for that exception type.
                var message = $"Request {request.ToLogString()} failed with exception of type {e.GetType()}. Please report that the outbound pipe handler {nameof(ThrowFulcrumExceptionOnFail)} should catch this exception type explicitly.";
                Log.LogError(message, e);
                throw new FulcrumAssertionFailedException(
                    message, e);
            }
        }
    }
}
