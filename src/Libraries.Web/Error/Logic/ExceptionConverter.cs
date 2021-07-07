using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Web.Logging;

namespace Nexus.Link.Libraries.Web.Error.Logic
{
    /// <summary>
    /// This class has conversion methods for converting between unsuccessful HTTP responses and Fulcrum exceptions.
    /// Fulcrum is only using three HTTP status codes for errors; 400, 500 and 503.
    /// This was based on the following blog article http://blog.restcase.com/rest-api-error-codes-101/
    /// </summary>
    public static class ExceptionConverter
    {
        private static readonly Dictionary<string, Func<string, Exception, FulcrumException>> FactoryMethodsCache = new Dictionary<string, Func<string, Exception, FulcrumException>>();
        private static readonly Dictionary<string, string> FulcrumErrorTargetExceptionType = new Dictionary<string, string>();
        private static readonly Dictionary<string, HttpStatusCode> HttpStatusCodesCache = new Dictionary<string, HttpStatusCode>();

        /// <summary>
        /// Use this method to add a new <see cref="FulcrumException"/>. This means that it will be included in converting.
        /// </summary>
        /// <param name="exceptionType">The type of the exception.</param>
        /// <param name="inboundStatusCode">The status code that it should be converted to if we convert it to an HTTP response.</param>
        /// <param name="outboundExceptionType">Fulcrum errors of type <paramref name="exceptionType"/> should be converted to this type of exception.</param>
        public static void AddFulcrumException(Type exceptionType, HttpStatusCode inboundStatusCode, string outboundExceptionType)
        {
            InternalContract.RequireNotNull(exceptionType, nameof(exceptionType));
            InternalContract.RequireNotNullOrWhiteSpace(outboundExceptionType, nameof(outboundExceptionType));
            var createDelegate = GetInstanceDelegate(exceptionType);
            var sourceException = createDelegate("test", (Exception)null);
            FactoryMethodsCache.Add(sourceException.Type, createDelegate);
            HttpStatusCodesCache.Add(sourceException.Type, inboundStatusCode);
            FulcrumErrorTargetExceptionType.Add(sourceException.Type, outboundExceptionType);
        }

        private static Func<string, Exception, FulcrumException> GetInstanceDelegate(Type exceptionType)
        {
            try
            {
                var methodInfo = exceptionType.GetMethod("Create");
                FulcrumAssert.IsNotNull(methodInfo);
                return (Func<string, Exception, FulcrumException>)Delegate.CreateDelegate(
                        typeof(Func<string, Exception, FulcrumException>), methodInfo);
            }
            catch (Exception e)
            {
                throw new FulcrumContractException(
                    $"The type {exceptionType.FullName} must have a factory method Create(string message, Exception innerException).",
                    e);
            }
        }

        static ExceptionConverter()
        {
            // Core
            AddFulcrumException(typeof(FulcrumAssertionFailedException), HttpStatusCode.InternalServerError, FulcrumResourceException.ExceptionType);
            AddFulcrumException(typeof(FulcrumResourceException), HttpStatusCode.BadGateway, FulcrumResourceException.ExceptionType);
            AddFulcrumException(typeof(FulcrumContractException), HttpStatusCode.InternalServerError, FulcrumResourceException.ExceptionType);
            AddFulcrumException(typeof(FulcrumNotImplementedException), HttpStatusCode.InternalServerError, FulcrumResourceException.ExceptionType);
            AddFulcrumException(typeof(FulcrumTryAgainException), HttpStatusCode.InternalServerError, FulcrumTryAgainException.ExceptionType);
            AddFulcrumException(typeof(FulcrumBusinessRuleException), HttpStatusCode.BadRequest, FulcrumBusinessRuleException.ExceptionType);
            AddFulcrumException(typeof(FulcrumConflictException), HttpStatusCode.BadRequest, FulcrumConflictException.ExceptionType);
            AddFulcrumException(typeof(FulcrumNotFoundException), HttpStatusCode.BadRequest, FulcrumNotFoundException.ExceptionType);

            // WebApi
            AddFulcrumException(typeof(FulcrumServiceContractException), HttpStatusCode.BadRequest, FulcrumContractException.ExceptionType);
            AddFulcrumException(typeof(FulcrumUnauthorizedException), HttpStatusCode.BadRequest, FulcrumUnauthorizedException.ExceptionType);
            AddFulcrumException(typeof(FulcrumForbiddenAccessException), HttpStatusCode.BadRequest, FulcrumForbiddenAccessException.ExceptionType);
            AddFulcrumException(typeof(FulcrumAcceptedException), HttpStatusCode.Accepted, FulcrumAcceptedException.ExceptionType);
        }

        /// <summary>
        /// Convert an exception (<paramref name="fulcrumException"/>) into a <see cref="FulcrumError"/>.
        /// </summary>
        public static FulcrumError ToFulcrumError(FulcrumException fulcrumException)
        {
            return ToFulcrumError(fulcrumException, true);
        }

        /// <summary>
        /// Convert an exception (<paramref name="exception"/>) into a <see cref="FulcrumError"/>.
        /// </summary>
        public static FulcrumError ToFulcrumError(Exception exception, bool topLevelMustBeFulcrumException)
        {
            if (exception == null) return null;
            var fulcrumError = new FulcrumError();
            if ((exception is FulcrumException fulcrumException))
            {
                fulcrumError.CopyFrom(fulcrumException);
            }
            else
            {

                fulcrumError.TechnicalMessage = exception.Message;
                fulcrumError.InstanceId = Guid.NewGuid().ToString();
                fulcrumError.Type = exception.GetType().FullName;
            }
            fulcrumError.InnerError = ToFulcrumError(exception.InnerException, false);
            fulcrumError.InnerInstanceId = fulcrumError.InnerError?.InstanceId;
            return fulcrumError;
        }

        public static async Task<FulcrumError> ToFulcrumErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode != HttpStatusCode.Accepted) return null;
            }

            var contentAsString = "";
            if (response.Content != null)
            {
                await response.Content?.LoadIntoBufferAsync();
                contentAsString = await response.Content?.ReadAsStringAsync();
                var fulcrumError = await FulcrumErrorFromContentAsync(response, contentAsString, cancellationToken);
                if (fulcrumError != null) return fulcrumError;
            }

            return CreateFulcrumErrorFromHttpStatusCode(response.StatusCode, contentAsString, response);
        }

        private static Task<FulcrumError> FulcrumErrorFromContentAsync(HttpResponseMessage response,
            string contentAsString, CancellationToken cancellationToken)
        {
            InternalContract.RequireNotNull(contentAsString, nameof(contentAsString));
            var fulcrumError = SafeParse<FulcrumError>(contentAsString);
            if (fulcrumError?.Type != null)
            {
                try
                {
                    fulcrumError.Validate(CodeLocation.AsString());
                }
                catch (ValidationException)
                {
                    // Intentionally ignore this validation problem. Just means that this was not to be considered a FulcrumError.
                }

                try
                {
                    ValidateStatusCode(response.StatusCode, fulcrumError);
                    return Task.FromResult(fulcrumError);
                }
                catch (FulcrumException e)
                {
                    // We will just log this. It is important that we still try to use the response in some way.
                    Log.LogWarning($"{response.RequestMessage.ToLogStringAsync(response, cancellationToken: cancellationToken)}\r{e.Message}\r{fulcrumError.ToLogString()}");
                }
            }

            return Task.FromResult((FulcrumError)null);
        }

        private static FulcrumError CreateFulcrumErrorFromHttpStatusCode(HttpStatusCode statusCode, string contentAsString,
            HttpResponseMessage response)
        {
            var shortContent = contentAsString;
            if (shortContent.Length > 160)
            {
                Log.LogVerbose($"Truncating failed response content to 160 characters. This was the original content:\r{contentAsString}");
                shortContent = $"content (truncated): {contentAsString.Substring(0, 160)}";
            }

            var content = string.IsNullOrWhiteSpace(shortContent) ? "no content" : $"content {shortContent}";
            var fulcrumError = new FulcrumError
            {
                CorrelationId = FulcrumApplication.Context.CorrelationId,
                FriendlyMessage =
                    $"A call to a remote service did not succeed and the service did not return a FulcrumError message. It returned status code {statusCode.ToLogString()}.",
                InstanceId = Guid.NewGuid().ToString(),
                IsRetryMeaningful = false,
                ServerTechnicalName = response?.RequestMessage?.RequestUri?.Host,
                TechnicalMessage = $"{response?.RequestMessage?.ToLogString()} returned {response?.StatusCode} with {content}"
            };

            var statusCodeAsInt = (int)statusCode;
            if (statusCodeAsInt >= 500)
            {
                switch (statusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        fulcrumError.Type = FulcrumAssertionFailedException.ExceptionType;
                        break;
                    case HttpStatusCode.NotImplemented:
                    case HttpStatusCode.HttpVersionNotSupported:
                        fulcrumError.Type = FulcrumNotImplementedException.ExceptionType;
                        break;
                    case HttpStatusCode.BadGateway:
                        fulcrumError.Type = FulcrumResourceException.ExceptionType;
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.GatewayTimeout:
                        fulcrumError.Type = FulcrumTryAgainException.ExceptionType;
                        fulcrumError.IsRetryMeaningful = true;
                        break;
                    default:
                        fulcrumError.Type = FulcrumAssertionFailedException.ExceptionType;
                        break;
                }
            }
            else if (statusCodeAsInt >= 400)
            {
                switch (statusCode)
                {
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.PaymentRequired:
                    case HttpStatusCode.MethodNotAllowed:
                    case HttpStatusCode.NotAcceptable:
                        fulcrumError.Type = FulcrumServiceContractException.ExceptionType;
                        break;
                    case HttpStatusCode.NotFound:
                        // TODO: Introduce FulcrumUnavailableException
                        fulcrumError.Type = FulcrumServiceContractException.ExceptionType;
                        break;
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.ProxyAuthenticationRequired:
                        fulcrumError.Type = FulcrumUnauthorizedException.ExceptionType;
                        break;
                    case HttpStatusCode.Forbidden:
                        fulcrumError.Type = FulcrumForbiddenAccessException.ExceptionType;
                        break;
                    case HttpStatusCode.RequestTimeout:
                        fulcrumError.Type = FulcrumTryAgainException.ExceptionType;
                        fulcrumError.IsRetryMeaningful = true;
                        break;
                    case HttpStatusCode.Conflict:
                        fulcrumError.Type = FulcrumConflictException.ExceptionType;
                        break;
                    case HttpStatusCode.Gone:
                        fulcrumError.Type = FulcrumNotFoundException.ExceptionType;
                        break;
                    default:
                        if (statusCodeAsInt == 429) // Too many requests
                        {
                            fulcrumError.Type = FulcrumTryAgainException.ExceptionType;
                            fulcrumError.IsRetryMeaningful = true;
                            fulcrumError.RecommendedWaitTimeInSeconds = 30;
                        }
                        else
                        {
                            fulcrumError.Type = FulcrumServiceContractException.ExceptionType;
                        }

                        break;
                }
            }
            else if (statusCodeAsInt >= 300)
            {
                fulcrumError.Type = FulcrumServiceContractException.ExceptionType;
            }
            else if (statusCodeAsInt == 202)
            {
                fulcrumError.Type = FulcrumAcceptedException.ExceptionType;
            }
            else
            {
                FulcrumAssert.Fail($"Could not convert HTTP status code {statusCode.ToLogString()} into a FulcrumError.");
            }

            return fulcrumError;
        }

        /// <summary>
        /// Convert an HTTP response (<paramref name="response"/>) into a <see cref="FulcrumException"/>.
        /// </summary>
        public static async Task<FulcrumException> ToFulcrumExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            var fulcrumError = await ToFulcrumErrorAsync(response, cancellationToken);
            if (fulcrumError == null) return null;
            var fulcrumException = ToFulcrumException(fulcrumError);
            FulcrumAssert.IsNotNull(fulcrumException, $"Could not convert the following {nameof(FulcrumError)} to a {nameof(FulcrumException)}:\r {ToJsonString(fulcrumError, Formatting.Indented)}");
            return fulcrumException;
        }

        /// <summary>
        /// Convert a <see cref="FulcrumError"/> (<paramref name="error"/>) into a <see cref="FulcrumException"/>.
        /// </summary>
        public static FulcrumException ToFulcrumException(FulcrumError error)
        {
            return ToFulcrumException(error, true);
        }

        /// <summary>
        /// Convert a <see cref="FulcrumError"/> (<paramref name="error"/>) into a <see cref="FulcrumException"/>.
        /// </summary>
        private static FulcrumException ToFulcrumException(FulcrumError error, bool convertType)
        {
            if (error == null) return null;
            var targetType = error.Type;
            if (convertType)
            {
                if (!FulcrumErrorTargetExceptionType.ContainsKey(error.Type))
                {
                    var message =
                        $"The error type ({error.Type}) was not recognized: {ToJsonString(error, Formatting.Indented)}. Add it to {typeof(ExceptionConverter).FullName} if you want it to be converted.";
                    return new FulcrumAssertionFailedException(message);
                }

                targetType = FulcrumErrorTargetExceptionType[error.Type];
            }

            if (!FactoryMethodsCache.ContainsKey(targetType))
            {
                if (!convertType) return null;
                var message = $"The error type ({targetType}) was not recognized. Add it to {typeof(ExceptionConverter).FullName} if you want it to be converted.";
                return new FulcrumAssertionFailedException(message);
            }
            var factoryMethod = FactoryMethodsCache[targetType];

            var typeHasChanged = targetType != error.Type;

            var innerFulcrumException = ToFulcrumException(typeHasChanged ? error : error.InnerError, false);
            var fulcrumException = factoryMethod(error.TechnicalMessage, innerFulcrumException);
            if (!typeHasChanged) fulcrumException.CopyFrom(error);
            return fulcrumException;
        }

        /// <summary>
        /// Transform a <see cref="FulcrumException"/> (<paramref name="source"/>) into a new <see cref="FulcrumException"/>.
        /// </summary>
        /// <param name="source">The exception to transform.</param>
        /// <param name="serverTechnicalName">The server that the </param>
        /// <returns>The same <paramref name="source"/> unless it was an exception that needs to be transformed; then a new exception is returned.</returns>
        /// <remarks>If <paramref name="source"/> had a null ServerTechnicalName it will be set to <paramref name="serverTechnicalName"/> as a side effect.</remarks>
        [Obsolete("Not used anymore.", true)]
        public static FulcrumException FromServiceToBll(FulcrumException source, string serverTechnicalName = null)
        {
            if (source == null) return null;
            source.ServerTechnicalName = source.ServerTechnicalName ?? serverTechnicalName;
            switch (source.Type)
            {
                case FulcrumAssertionFailedException.ExceptionType:
                case FulcrumNotImplementedException.ExceptionType:
                    return new FulcrumAssertionFailedException($"Did not expect {source.ServerTechnicalName ?? "server"} to return the following error: {source.Message}", source);
                case FulcrumServiceContractException.ExceptionType:
                    return new FulcrumAssertionFailedException($"Bad call to { source.ServerTechnicalName ?? "Server" }: { source.Message}", source);
                case FulcrumUnauthorizedException.ExceptionType:
                    return new FulcrumAssertionFailedException($"Unauthorized call to {source.ServerTechnicalName ?? "server"}: {source.Message}", source);
                default:
                    return source;
            }
        }

        private static void ValidateStatusCode(HttpStatusCode statusCode, FulcrumError error)
        {
            var expectedStatusCode = ToHttpStatusCode(error);
            if (expectedStatusCode == null)
            {
                throw new FulcrumAssertionFailedException(
                    $"The Type of the content could not be converted to an HTTP status code: {ToJsonString(error, Formatting.Indented)}.");
            }
            if (expectedStatusCode != statusCode)
            {
                throw new FulcrumAssertionFailedException(
                    $"The HTTP error response had status code {statusCode}, but was expected to have {expectedStatusCode.Value}, due to the Type in the content: \"{ToJsonString(error, Formatting.Indented)}");
            }
        }

        /// <summary>
        /// Checks a dictionary for the proper <see cref="HttpStatusCode"/> for <paramref name="fulcrumException"/>.
        /// </summary>
        public static HttpStatusCode? ToHttpStatusCode(FulcrumException fulcrumException)
        {
            var error = ToFulcrumError(fulcrumException, true);
            return ToHttpStatusCode(error);
        }

        /// <summary>
        /// Checks a dictionary for the proper <see cref="HttpStatusCode"/> for <paramref name="error"/>.
        /// </summary>
        public static HttpStatusCode? ToHttpStatusCode(FulcrumError error)
        {
            if (error == null) return null;
            if (!HttpStatusCodesCache.ContainsKey(error.Type)) return null;
            return HttpStatusCodesCache[error.Type];
        }

        /// <summary>
        /// Converts <paramref name="fulcrumError"/> into a JSON string.
        /// </summary>
        /// <returns></returns>
        public static string ToJsonString(IFulcrumError fulcrumError, Formatting formatting)
        {
            return JObject.FromObject(fulcrumError).ToString(formatting);
        }

        /// <summary>
        /// Parse a JSON string and converts it into an object.
        /// </summary>
        /// <remarks>
        /// Catches all exceptions and then returns null.
        /// </remarks>
        public static T SafeParse<T>(string jsonObject)
            where T : class
        {
            if (jsonObject == null) return null;
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonObject);
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}