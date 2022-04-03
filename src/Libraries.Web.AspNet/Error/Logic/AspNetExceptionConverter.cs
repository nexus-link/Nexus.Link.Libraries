using System;
using System.Net;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Error;
using Nexus.Link.Libraries.Web.Error.Logic;
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Net.Http;
using System.Text;
#endif

//TODO: ExceptionToFulcrumResponse

namespace Nexus.Link.Libraries.Web.AspNet.Error.Logic
{
    /// <summary>
    /// This class has conversion methods for converting between unsuccessful HTTP responses and Fulcrum exceptions.
    /// Fulcrum is only using three HTTP status codes for errors; 400, 500 and 503.
    /// This was based on the following blog article http://blog.restcase.com/rest-api-error-codes-101/
    /// </summary>
    public static class AspNetExceptionConverter
    {
#if NETCOREAPP
        /// <summary>
        /// Convert an exception (<paramref name="e"/>) into an HTTP response message.
        /// </summary>
        public static ContentResult ToContentResult(Exception e)
        {
            InternalContract.RequireNotNull(e, nameof(e));
            var statusAndContent = ToStatusAndContent(e);

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
        public static HttpResponseMessage ToHttpResponseMessage(Exception e)
        {
            InternalContract.RequireNotNull(e, nameof(e));
            var statusAndContent = ToStatusAndContent(e);
            var stringContent = new StringContent(statusAndContent.Content, Encoding.UTF8);
            var response = new HttpResponseMessage(statusAndContent.StatusCode)
            {
                Content = stringContent
            };
            return response;
        }
#endif

        private static StatusAndContent ToStatusAndContent(Exception e)
        {
            if (e is RequestAcceptedException acceptedException)
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
            if (e is RequestPostponedException postponedException)
            {
                var postponedContent = new RequestPostponedContent()
                {
                    TryAgain = postponedException.TryAgain,
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
            if (!(e is FulcrumException fulcrumException))
            {
                var message = $"Application threw an exception that didn't inherit from {typeof(FulcrumException)}.\r{e.GetType().FullName}: {e.Message}\rFull exception:\r{e}";
                Log.LogError(message, e);
                fulcrumException = new FulcrumAssertionFailedException(message, e);
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
}