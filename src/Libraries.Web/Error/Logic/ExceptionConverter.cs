using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Core.Logging;
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
                return (Func<string, Exception, FulcrumException>) Delegate.CreateDelegate(
                        typeof(Func<string, Exception, FulcrumException>), methodInfo);
            }
            catch (Exception e)
            {
                throw new FulcrumContractException(
                    $"The type {exceptionType.FullName} must have a factory method Create(string message, Exception innerException).",
                    e);
            }
        }

        /// <summary>
        /// Use this method to add a new <see cref="FulcrumException"/>. This means that it will be included in converting.
        /// </summary>
        /// <param name="exceptionType">The type of the exception.</param>
        /// <param name="statusCode">The status code that it should be converted to if we convert it to an HTTP response.</param>
        [Obsolete("Use the overload with three arguments", true)]
        public static void AddFulcrumException(Type exceptionType, HttpStatusCode? statusCode = null)
        {
            InternalContract.RequireNotNull(exceptionType, nameof(exceptionType));
            InternalContract.RequireNotNull(statusCode, nameof(statusCode));
            var createDelegate = GetInstanceDelegate(exceptionType);
            var exception = createDelegate("test", (Exception) null);
            // ReSharper disable once PossibleInvalidOperationException
            AddFulcrumException(exceptionType, statusCode.Value, exception.Type);
        }

        static ExceptionConverter()
        {
            // Core
            AddFulcrumException(typeof(FulcrumAssertionFailedException), HttpStatusCode.InternalServerError, FulcrumResourceException.ExceptionType);
            AddFulcrumException(typeof(FulcrumResourceException), HttpStatusCode.InternalServerError, FulcrumResourceException.ExceptionType);
            AddFulcrumException(typeof(FulcrumResourceContractException), HttpStatusCode.InternalServerError, FulcrumResourceException.ExceptionType);
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
        }

        /// <summary>
        /// Convert an exception (<paramref name="e"/>) into a <see cref="FulcrumError"/>.
        /// </summary>
        public static FulcrumError ToFulcrumError(FulcrumException fulcrumException)
        {
            if (fulcrumException == null) return null;

            var error = new FulcrumError();
            error.CopyFrom(fulcrumException);
            error.InnerError = ToFulcrumError(fulcrumException.InnerException, true);
            error.InnerInstanceId = error.InnerError?.InstanceId;
            return error;
        }

        /// <summary>
        /// Convert an exception (<paramref name="e"/>) into a <see cref="FulcrumError"/>.
        /// </summary>
        private static FulcrumError ToFulcrumError(Exception e, bool alsoNonFulcrumExceptions = false)
        {
            if (e == null) return null;
            if ((e is FulcrumException fulcrumException))
            {
                var error = new FulcrumError();
                error.CopyFrom(fulcrumException);
                error.InnerError = ToFulcrumError(fulcrumException.InnerException, true);
                error.InnerInstanceId = error.InnerError?.InstanceId;
                return error;
            }

            if (!alsoNonFulcrumExceptions) return null;

            var fulcrumError = new FulcrumError
            {
                TechnicalMessage = e.Message,
                InstanceId = Guid.NewGuid().ToString(),
                Type = e.GetType().FullName
            };
            return fulcrumError;
        }

        public static async Task<FulcrumError> ToFulcrumErrorAsync(HttpResponseMessage response)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            if (response.IsSuccessStatusCode) return null;

            var contentAsString = "";
            if (response.Content != null)
            {
                await response.Content?.LoadIntoBufferAsync();
                contentAsString = await response.Content?.ReadAsStringAsync();
                var fulcrumError = Parse<FulcrumError>(contentAsString);
                if (fulcrumError?.Type != null) return fulcrumError;
            }

            return ToFulcrumError(response.StatusCode, contentAsString, response);
        }

        private static FulcrumError ToFulcrumError(HttpStatusCode statusCode, string contentAsString,
            HttpResponseMessage response)
        {
            var shortContent = contentAsString;
            if (shortContent.Length > 160)
            {
                Log.LogInformation($"Truncating failed response content to 160 characters. This was the original content:\r{contentAsString}");
                shortContent = $"Truncated content: {contentAsString.Substring(0, 160)}";
            }
            var fulcrumError = new FulcrumError
            {
                CorrelationId = FulcrumApplication.Context.CorrelationId,
                FriendlyMessage =
                    $"A call to a remote service did not succeed and the service did not return a FulcrumError message. It returned status code {statusCode.ToLogString()}.",
                InstanceId = Guid.NewGuid().ToString(),
                IsRetryMeaningful = false,
                ServerTechnicalName = response?.RequestMessage?.RequestUri?.Host,
                TechnicalMessage = $"{response?.StatusCode}: {shortContent}"
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
                    case HttpStatusCode.NotFound:
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
                        fulcrumError.Type = FulcrumServiceContractException.ExceptionType;
                        break;
                }
            }
            else if (statusCodeAsInt >= 300)
            {
                fulcrumError.Type = FulcrumServiceContractException.ExceptionType;
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
        public static async Task<FulcrumException> ToFulcrumExceptionAsync(HttpResponseMessage response)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            var fulcrumError = await ToFulcrumErrorAsync(response);
            if (fulcrumError == null) return null;
            ValidateStatusCode(response.StatusCode, fulcrumError);
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

            var typeHasChanged = targetType != error.Type;

            if (!FactoryMethodsCache.ContainsKey(targetType))
            {
                var message = $"The error type ({targetType}) was not recognized. Add it to {typeof(ExceptionConverter).FullName} if you want it to be converted.";
                return new FulcrumAssertionFailedException(message);
            }
            var factoryMethod = FactoryMethodsCache[targetType];

            var fulcrumException = factoryMethod(error.TechnicalMessage, null);
            fulcrumException.CopyFrom(error);
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
            var error = ToFulcrumError(fulcrumException);
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
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static T Parse<T>(string jsonObject)
            where T : class
        {
            if (jsonObject == null) return null;
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonObject);
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

    }
}