using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Json;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.Error;
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
                requestDescription = $"OUT request-response {await request.ToLogStringAsync(response, cancellationToken: cancellationToken)}";

                if (response.StatusCode == HttpStatusCode.Accepted && response.Content != null)
                {
                    await response.Content.LoadIntoBufferAsync();
                    var content = await response.Content.ReadAsStringAsync();
                    var acceptInfo = JsonHelper.SafeDeserializeObject<RequestAcceptedContent>(content);
                    if (acceptInfo?.RequestId != null)
                    {
                        throw new RequestAcceptedException(acceptInfo.RequestId)
                        {
                            PollingUrl = acceptInfo.PollingUrl,
                            RegisterCallbackUrl = acceptInfo.RegisterCallbackUrl
                        };
                    }
                    var postponeInfo = JsonHelper.SafeDeserializeObject<RequestPostponedContent>(content);
                    if (postponeInfo?.WaitingForRequestIds != null)
                    {
                        var timeSpan = postponeInfo.TryAgainAfterMinimumSeconds.HasValue
                            ? TimeSpan.FromSeconds(postponeInfo.TryAgainAfterMinimumSeconds.Value)
                            : (TimeSpan?)null;
                        var exception =  new InternalRequestPostponedException(postponeInfo?.WaitingForRequestIds)
                        {
                            TryAgainAfterMinimumTimeSpan = timeSpan,
                            ReentryAuthentication = postponeInfo.ReentryAuthentication,
                            Backoff = postponeInfo.Backoff
                        };
                        throw exception;
                    }

                    return response;
                }
                if (response.IsSuccessStatusCode) return response;
                fulcrumException = await ExceptionConverter.ToFulcrumExceptionAsync(response, cancellationToken);
                if (fulcrumException == null)
                {
                    return response;
                }
            }
            catch (FulcrumException e)
            {
                Log.LogError($"{requestDescription} threw the exception {e.GetType().Name}: {e.TechnicalMessage}", e);
                throw;
            }
            catch (RequestAcceptedException)
            {
                Log.LogInformation($"{requestDescription} was converted to (and threw) the exception {nameof(RequestAcceptedException)}");
                throw;
            }
            catch (RequestPostponedException)
            {
                Log.LogInformation($"{requestDescription} was converted to (and threw) the exception {nameof(RequestPostponedException)}");
                throw;
            }
            catch (TaskCanceledException e)
            {
                var message = $"{requestDescription} was cancelled.";
                Log.LogWarning(message, e);
                throw new FulcrumTryAgainException(message, e);
            }
            catch (JsonReaderException e)
            {
                var message = $"{requestDescription} failed: {e.Message}.";
                Log.LogWarning(message, e);
                throw new FulcrumResourceException(message, e);
            }
            catch (HttpRequestException e)
            {
                var message = $"{requestDescription} failed: {e.Message}.";
                Log.LogWarning(message, e);
                throw new FulcrumResourceException(message, e)
                {
                    IsRetryMeaningful = true,
                    RecommendedWaitTimeInSeconds = 10
                };
            }
            catch (Exception e)
            {
                // If we end up here, we probably need to add another catch statement for that exception type.
                var message =
                    $"{requestDescription} failed with an exception of type {e.GetType().FullName}. Please report that the outbound pipe handler {nameof(ThrowFulcrumExceptionOnFail)} should catch this exception type explicitly.";
                Log.LogError(message, e);
                throw new FulcrumAssertionFailedException(message, e);
            }

            var severityLevel = (int)response.StatusCode >= 500
                ? LogSeverityLevel.Error
                : (int)response.StatusCode >= 400
                    ? LogSeverityLevel.Warning
                    : LogSeverityLevel.Information;
            Log.LogOnLevel(severityLevel, $"{requestDescription} was converted to (and threw) the exception {fulcrumException.GetType().Name}: {fulcrumException.TechnicalMessage}", fulcrumException);
            throw fulcrumException;
        }
    }
}
