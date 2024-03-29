﻿using System;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Error.Model
{
    /// <summary>
    /// Information that will be returned when a REST service returns a non successful HTTP status code
    /// </summary>
    /// <remarks>
    /// Inspired by the follwing articles
    /// http://blog.restcase.com/rest-api-error-codes-101/
    /// https://stormpath.com/blog/spring-mvc-rest-exception-handling-best-practices-part-1
    /// </remarks>
    public class FulcrumError : IFulcrumError, ILoggable
    {
        /// <inheritdoc />
        public string TechnicalMessage { get; set; }

        /// <inheritdoc />
        public string FriendlyMessage { get; set; }

        /// <inheritdoc />
        public string MoreInfoUrl { get; set; }

        /// <inheritdoc />
        public bool IsRetryMeaningful { get; set; }

        /// <inheritdoc />
        public double RecommendedWaitTimeInSeconds { get; set; }

        /// <inheritdoc />
        public string ServerTechnicalName { get; set; }

        /// <inheritdoc />
        public string InstanceId { get; set; }

        /// <inheritdoc />
        public string InnerInstanceId { get; set; }

        /// <inheritdoc />
        public string ErrorLocation { get; set; }

        /// <inheritdoc />
        public string Code { get; set; }

        /// <inheritdoc />
        public string Type { get; set; }

        /// <inheritdoc />
        public string CorrelationId { get; set; }

        /// <summary>
        /// A place where we save Exception.Data
        /// </summary>
        public string SerializedData { get; set; }

        /// <summary>
        /// Something like an inner exception; if this fulcrum error happens when dealing with another error, this is that error.
        /// </summary>
        public FulcrumError InnerError { get; set; }

        /// <inheritdoc />
        public IFulcrumError CopyFrom(IFulcrumError fulcrumError)
        {
            TechnicalMessage = fulcrumError.TechnicalMessage;
            FriendlyMessage = fulcrumError.FriendlyMessage;
            MoreInfoUrl = fulcrumError.MoreInfoUrl;
            IsRetryMeaningful = fulcrumError.IsRetryMeaningful;
            RecommendedWaitTimeInSeconds = fulcrumError.RecommendedWaitTimeInSeconds;
            ServerTechnicalName = fulcrumError.ServerTechnicalName;
            InstanceId = fulcrumError.InstanceId;
            InnerInstanceId = fulcrumError.InnerInstanceId;
            Code = fulcrumError.Code;
            ErrorLocation = fulcrumError.ErrorLocation;
            Type = fulcrumError.Type;
            CorrelationId = fulcrumError.CorrelationId;
            switch (fulcrumError)
            {
                case Exception sourceException:
                    SerializedData = JsonConvert.SerializeObject(sourceException.Data);
                    break;
                case FulcrumError sourceError:
                    SerializedData = sourceError.SerializedData;
                    break;
                default:
                    FulcrumAssert.Fail(CodeLocation.AsString());
                    break;
            }

            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return TechnicalMessage;
        }

        /// <inheritdoc />
        public string ToLogString()
        {
            return
                $"InstanceId: {InstanceId}, CorrelationId: {CorrelationId}, Type: {Type}, Message: {TechnicalMessage}";
        }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(TechnicalMessage, nameof(TechnicalMessage), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Type, nameof(Type), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InstanceId, nameof(InstanceId), errorLocation);
        }
    }
}
