using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Entities
{
    /// <summary>
    /// Exception object when request execution throw an exception
    /// </summary>
    public class ResponseException
    {
        /// <summary>
        /// If the request execution throw an exception, this is the name of the type of the exception.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// If the request execution throw an exception, this is message of that exception.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// The HttpResponse.
    /// </summary>
    public class HttpResponse : IValidatable
    {
        /// <summary>
        /// The metadata for the HttpResponse.
        /// </summary>
        public ResponseMetadata Metadata { get; set; } = new ResponseMetadata();

        // TODO: #254 This should be the executionId.
        /// <summary>
        /// The request id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The HTTP status
        /// </summary>
        public int? HttpStatus { get; set; }

        /// <summary>
        /// The HTTP content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The HTTP Headers
        /// </summary>
        public Dictionary<string, StringValues> Headers { get; set; }

        /// <summary>
        /// The Exception
        /// </summary>
        public ResponseException Exception { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(Metadata, nameof(Metadata), errorLocation);
            FulcrumValidate.IsValidated(Metadata, propertyPath, nameof(Metadata), errorLocation);
            if (Exception != null)
            {
                FulcrumValidate.IsTrue(!HttpStatus.HasValue, errorLocation,
                    $"{propertyPath}.{nameof(HttpStatus)} should be null when {propertyPath}.{nameof(Exception)} is not null");
                FulcrumValidate.IsNotNullOrWhiteSpace(Exception.Message, nameof(Exception.Message), errorLocation);
                FulcrumValidate.IsNotNullOrWhiteSpace(Exception.Name, nameof(Exception.Name), errorLocation);
                FulcrumValidate.IsTrue(Content == null, errorLocation, $"Expected {propertyPath}.{nameof(Content)} to be null when {Exception.Name} is not null.");
                FulcrumValidate.IsNotNull(Headers != null, nameof(Headers), errorLocation, $"Expected {propertyPath}.{nameof(Headers)} to be null when {Exception.Name} is not null.");
            }
            else
            {
                if (HttpStatus.HasValue)
                {
                    FulcrumValidate.IsGreaterThan(0, HttpStatus.Value, nameof(HttpStatus), errorLocation);
                }

                ValidateHeaders();
            }

            if (Metadata.RequestHasCompleted)
            {
                FulcrumValidate.IsTrue(HttpStatus > 0 || Exception != null, errorLocation,
                    $"When {propertyPath}.{nameof(Metadata)}.{nameof(Metadata.RequestHasCompleted)} = {Metadata.RequestHasCompleted}" +
                    $", then {propertyPath}.{nameof(HttpStatus)} or {propertyPath}.{nameof(Exception)} must be set.");

            }

            void ValidateHeaders()
            {
                if (Headers != null && Headers.Any())
                {
                    var index = 0;
                    foreach (var header in Headers)
                    {
                        index++;
                        FulcrumValidate.IsNotNullOrWhiteSpace(header.Key, "ignore", errorLocation,
                            $"Header {index} in {propertyPath}.{nameof(Headers)} had an empty key.");
                        FulcrumValidate.IsNotNullOrWhiteSpace(header.Value.ToString(), "ignore", errorLocation,
                            $"Header {header.Key} had an empty value.");
                        FulcrumValidate.IsTrue(header.Value.Any(), errorLocation,
                            $"Header {header.Key} had no values.");
                        FulcrumValidate.IsTrue(!header.Value.Any(string.IsNullOrWhiteSpace), errorLocation,
                            $"Header {header.Key} had an empty value.");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Describes a response that the client receives for an asynchronously executed httpRequest.
    /// </summary>
    public class ResponseMetadata : IValidatable
    {
        /// <summary>
        /// The number of executions.
        /// </summary>
        public int Executions { get; set; }

        /// <summary>
        /// Convenience property. True if the request has completed and we have a final response.
        /// </summary>
        public bool RequestHasCompleted { get; set; }

        /// <summary>
        /// Describing the reason why a request is considered to be completed.
        /// </summary>
        public RequestCompletionReasonEnum? CompletionReason { get; set; }

        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// The time in seconds user agent should wait before making trying to read the response again.
        /// </summary>
        public double RecommendedWaitingTimeInSeconds { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsGreaterThanOrEqualTo(0, Executions, nameof(Executions), errorLocation);
            FulcrumValidate.IsGreaterThanOrEqualTo(0.0, RecommendedWaitingTimeInSeconds, nameof(RecommendedWaitingTimeInSeconds), errorLocation);
        }
    }

    /// <summary>
    /// Enum describing the reason why a request is considered to be completed.
    /// </summary>
    public enum RequestCompletionReasonEnum
    {
        /// <summary>
        /// The request was executed successfully
        /// </summary>
        Success,
        /// <summary>
        /// The request did not complete before Request.ExecuteBefore
        /// </summary>
        Expired,
        /// <summary>
        /// The request failed and will not be retried
        /// </summary>
        Failed
    }
}
