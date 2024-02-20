using System;
using System.Net;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error;
using Nexus.Link.Libraries.Web.Error.Logic;
using Nexus.Link.Libraries.Web.Pipe;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Web.Serialization;

#else
using System.Net.Http;
using System.Text;
using System.Threading;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Error.Logic
{
    /// <summary>
    /// This class has conversion methods for converting between unsuccessful HTTP responses and Fulcrum exceptions.
    /// Fulcrum is only using three HTTP status codes for errors; 400, 500 and 503.
    /// This was based on the following blog article http://blog.restcase.com/rest-api-error-codes-101/
    /// </summary>
    public static class AspNetExceptionConverter
    {

        /// <summary>
        /// If a request takes shorter than this, and an <see cref="OperationCanceledException"/> is captured,
        /// we consider the request as cancelled by the client.
        /// </summary>
        public static TimeSpan WebServerExecutionTimeLimit { get; set; } = TimeSpan.FromSeconds(99);

#if NETCOREAPP
        public static async Task ConvertExceptionToResponseAsync(Exception exception, HttpResponse response, CancellationToken originalRequestToken = default)
        {
            InternalContract.RequireNotNull(exception, nameof(exception));
            InternalContract.RequireNotNull(response, nameof(response));

            var customHttpResponse = ConvertExceptionToCustomHttpResponse(exception, originalRequestToken);

            response.StatusCode = customHttpResponse.StatusCode;
            if (customHttpResponse.LocationUri != null)
            {
                response.Headers.Add("Location", customHttpResponse.LocationUri.OriginalString);
            }
            response.ContentType = customHttpResponse.ContentType;

            // ReSharper disable once MethodSupportsCancellation
            await response.WriteAsync(customHttpResponse.Content);
        }

        [Obsolete("Please use the overload with cancellation token. Warning since 2023-03-01.")]
        public static CustomHttpResponse ConvertExceptionToCustomHttpResponse(Exception exception)
        {
            return ConvertExceptionToCustomHttpResponse(exception, null);
        }

        public static CustomHttpResponse ConvertExceptionToCustomHttpResponse(Exception exception, CancellationToken? originalRequestToken)
        {
            InternalContract.RequireNotNull(exception, nameof(exception));

            var response = new CustomHttpResponse();
            if (exception is FulcrumHttpRedirectException redirectException)
            {
                response.Content = redirectException.Content;
                response.StatusCode = redirectException.HttpStatusCode;
                response.LocationUri = redirectException.LocationUri;
                response.ContentType = redirectException.ContentType;
            }
            else
            {
                var statusAndContent = ToStatusAndContent(exception, originalRequestToken);
                response.Content = statusAndContent.Content;
                response.StatusCode = (int)statusAndContent.StatusCode;
                response.ContentType = "application/json";
            }

            return response;
        }

        /// <summary>
        /// Convert an exception (<paramref name="e"/>) into an HTTP response message.
        /// </summary>
        [Obsolete("Has been replaced with ConvertExceptionToResponse.", true)]
        public static ContentResult ToContentResult(Exception e)
        {
            InternalContract.RequireNotNull(e, nameof(e));
            var statusAndContent = ToStatusAndContent(e, null);

            var res = new ContentResult
            {
                Content = statusAndContent.Content,
                ContentType = "application/json",
                StatusCode = (int)statusAndContent.StatusCode
            };

            return res;
        }


#else
        /// <summary>
        /// Convert an exception (<paramref name="e"/>) into an HTTP response message.
        /// </summary>
        [Obsolete("Please use the overload with cancellation token. Warning since 2023-03-31.")]
        public static HttpResponseMessage ToHttpResponseMessage(Exception e)
        {
            return ToHttpResponseMessage(e, null);
        }

        /// <summary>
        /// Convert an exception (<paramref name="e"/>) into an HTTP response message.
        /// </summary>
        public static HttpResponseMessage ToHttpResponseMessage(Exception e, CancellationToken? originalRequestToken)
        {
            InternalContract.RequireNotNull(e, nameof(e));
            HttpResponseMessage response;
            if (e is FulcrumHttpRedirectException redirectException)
            {
                var stringContent = new StringContent(redirectException.Content, Encoding.UTF8, redirectException.ContentType);
                response = new HttpResponseMessage((HttpStatusCode)redirectException.HttpStatusCode)
                {
                    Content = stringContent
                };
                response.Headers.Location = redirectException.LocationUri;
            }
            else
            {
                var statusAndContent = ToStatusAndContent(e, originalRequestToken);
                var stringContent = new StringContent(statusAndContent.Content, Encoding.UTF8, "application/json");
                response = new HttpResponseMessage(statusAndContent.StatusCode)
                {
                    Content = stringContent
                };
            }
            return response;
        }
#endif

        private static StatusAndContent ToStatusAndContent(Exception e, CancellationToken? originalRequestToken)
        {
            switch (e)
            {
                case RequestAcceptedException acceptedException:
                    {
                        var acceptedContent = new RequestAcceptedContent()
                        {
                            RequestId = acceptedException.RequestId,
                            PollingUrl = acceptedException.PollingUrl,
                            RegisterCallbackUrl = acceptedException.RegisterCallbackUrl
                        };
                        FulcrumAssert.IsValidated(acceptedContent, CodeLocation.AsString());
                        return new StatusAndContent
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            StatusCode = HttpStatusCode.Accepted,
                            Content = JsonConvert.SerializeObject(acceptedContent)
                        };
                    }
                case RequestPostponedException postponedException:
                    {
                        var seconds = postponedException.TryAgainAfterMinimumTimeSpan?.TotalSeconds;
                        var postponedContent = new RequestPostponedContent()
                        {
#pragma warning disable CS0618
                            TryAgain = postponedException.TryAgain,
#pragma warning restore CS0618
                            TryAgainAfterMinimumSeconds = seconds,
                            WaitingForRequestIds = postponedException.WaitingForRequestIds,
                            ReentryAuthentication = postponedException.ReentryAuthentication
                        };
                        FulcrumAssert.IsValidated(postponedContent, CodeLocation.AsString());
                        return new StatusAndContent
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            StatusCode = HttpStatusCode.Accepted,
                            Content = JsonConvert.SerializeObject(postponedContent)
                        };
                    }
            }

            if (e is not FulcrumException fulcrumException)
            {
                switch (e)
                {
                    case OperationCanceledException operationCanceledException:
                        if ((!originalRequestToken.HasValue || originalRequestToken.Value.IsCancellationRequested) &&
                            FulcrumApplication.Context.RequestStopwatch != null &&
                            FulcrumApplication.Context.RequestStopwatch.Elapsed < WebServerExecutionTimeLimit)
                        {
                            fulcrumException = new FulcrumServiceContractException(
                                "The request execution was probably interrupted by the client. " +
                                "We currently classify this as a client side error.", operationCanceledException)
                            {
                                Code = Constants.CanceledByClient
                            };
                            return new StatusAndContent
                            {
                                StatusCode = (HttpStatusCode)499,
                                Content = ExceptionConverter.ToJsonString(fulcrumException, Formatting.Indented)
                            };
                        }

                        fulcrumException = new FulcrumAssertionFailedException(
                            $"The request execution was interrupted due to an internal {typeof(OperationCanceledException)}. " +
                            "We classify this as a server side error.", operationCanceledException);
                        break;
                    default:
                        var message = $"Application threw an exception that didn't inherit from {typeof(FulcrumException)}.\r{e.GetType().FullName}: {e.Message}\rFull exception:\r{e}";
                        Log.LogError(message, e);
                        fulcrumException = new FulcrumAssertionFailedException(message, e);
                        break;
                }
            }

            if (e is FulcrumHttpRedirectException)
            {
                // FulcrumHttpRedirectException should be handled outside this method, leading to not calling this method at all
                FulcrumAssert.Fail($"Did not expect an exception of type {nameof(FulcrumHttpRedirectException)} here.");
            }

            var error = ExceptionConverter.ToFulcrumError(fulcrumException, true);
            var statusCode = ExceptionConverter.ToHttpStatusCode(error);
            FulcrumAssert.IsNotNull(statusCode, CodeLocation.AsString());
            Log.LogVerbose(
                $"{error.Type} => HTTP status {statusCode}");
            var content = ExceptionConverter.ToJsonString(error, Formatting.Indented);
            return new StatusAndContent
            {
                // ReSharper disable once PossibleInvalidOperationException
                StatusCode = statusCode.Value,
                Content = content
            };
        }

        private static StatusAndContent FatalErrorAsActionResult(Exception e)
        {
            var message =
                $"FATAL error in Xlent.Lever.Libraries.Web.AspNetApi.GlobalExceptionHandler. Could not convert an exception to an HTTP response message. Additional information: {e.Message}";

            var res = new StatusAndContent
            {
                Content = message,
                StatusCode = HttpStatusCode.InternalServerError

            };
            return res;
        }

        private class StatusAndContent
        {
            public HttpStatusCode StatusCode;
            public string Content;
        }
    }

    public class CustomHttpResponse
    {
        public int StatusCode;
        public string ContentType;
        public string Content;
        public Uri LocationUri;
    }
}