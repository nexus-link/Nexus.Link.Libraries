using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Model;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The base class for all Fulcrum exceptions
    /// </summary>
    public abstract class FulcrumException : Exception, IFulcrumError, ILoggable
    {
        /// <summary>
        /// The current server name. Can be set by calling <see cref="Initialize"/>.
        /// Will automatically be copied to the the field <see cref="ServerTechnicalName"/> for every new error.
        /// </summary>
        private static string _serverTechnicalName;

        /// <summary>
        /// Mandatory technical information that a developer might find useful.
        /// This is where you might include exception messages, stack traces, or anything else that you
        /// think will help a developer.
        /// </summary>
        /// <remarks>
        /// This message is not expected to contain any of the codes or identifiers that are already contained
        /// in this error type, sucha as the error <see cref="Code"/> or the <see cref="InstanceId"/>.
        /// </remarks>
        /// <remarks>
        /// If this property has not been set, the recommendation is to treat the <see cref="System.Exception.Message"/>
        /// property as the technical message.
        /// </remarks>
        public string TechnicalMessage { get; set; }

        /// <inheritdoc />
        public virtual string FriendlyMessage { get; set; }

        /// <inheritdoc />
        public string MoreInfoUrl { get; set; }

        /// <inheritdoc />
        public virtual bool IsRetryMeaningful { get; set; }

        /// <inheritdoc />
        public double RecommendedWaitTimeInSeconds { get; set; }

        /// <inheritdoc />
        public string ServerTechnicalName { get; set; }

        /// <inheritdoc />
        public string InstanceId { get; private set; }

        /// <inheritdoc />
        public string InnerInstanceId { get; private set; }

        /// <inheritdoc />
        public string ErrorLocation { get; set; }

        /// <inheritdoc />
        public string Code { get; set; }

        /// <inheritdoc />
        public virtual string Type { get; private set; }

        /// <inheritdoc />
        public string CorrelationId { get; set; }

        /// <summary>
        /// Empty constructor
        /// </summary>
        protected FulcrumException() : this(null, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected FulcrumException(string message) : this(message, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected FulcrumException(string message, Exception innerException) : base(message, innerException)
        {
            TechnicalMessage = message;
            InstanceId = Guid.NewGuid().ToString();
            ServerTechnicalName = _serverTechnicalName;
            CorrelationId = FulcrumApplication.Context.CorrelationId;
            if (!(innerException is IFulcrumError innerError))
            {
                return;
            }
            RecommendedWaitTimeInSeconds = innerError.RecommendedWaitTimeInSeconds;
            InnerInstanceId = innerError.InstanceId;
        }

        /// <inheritdoc />
        public IFulcrumError CopyFrom(IFulcrumError fulcrumError)
        {
            InternalContract.RequireNotNull(fulcrumError, nameof(fulcrumError));
            TechnicalMessage = TechnicalMessage ?? fulcrumError.TechnicalMessage;
            IsRetryMeaningful = fulcrumError.IsRetryMeaningful;
            RecommendedWaitTimeInSeconds = fulcrumError.RecommendedWaitTimeInSeconds;
            if (fulcrumError.Type != Type)
            {
                InnerInstanceId = fulcrumError.InstanceId;
                return this;
            }

            CorrelationId = fulcrumError.CorrelationId;
            InstanceId = fulcrumError.InstanceId;
            InnerInstanceId = fulcrumError.InnerInstanceId;
            ServerTechnicalName = fulcrumError.ServerTechnicalName;
            FriendlyMessage = fulcrumError.FriendlyMessage;
            MoreInfoUrl = fulcrumError.MoreInfoUrl ?? MoreInfoUrl;
            Code = fulcrumError.Code ?? Code;
            ErrorLocation = fulcrumError.ErrorLocation;

            switch (fulcrumError)
            {
                case Exception sourceException:
                    // https://stackoverflow.com/questions/144957/using-exception-data
                    foreach (DictionaryEntry kvp in sourceException.Data)
                    {
                        Data[kvp.Key] = kvp.Value;
                    }
                    break;
                case FulcrumError sourceError:
                    if (!string.IsNullOrWhiteSpace(sourceError.SerializedData))
                    {
                        var data = JsonConvert
                            .DeserializeObject<Dictionary<string, object>>(sourceError.SerializedData);
                        foreach (var keyValuePair in data)
                        {
                            Data[keyValuePair.Key] = keyValuePair.Value;
                        }
                    }
                    break;
                default:
                    FulcrumAssert.Fail(CodeLocation.AsString());
                    break;
            }
            return this;
        }

        /// <summary>
        /// Sets the server technical name. This name will be used as a default for all new FulcrumExceptions.
        /// </summary>
        /// <param name="serverTechnicalName"></param>
        public static void Initialize(string serverTechnicalName)
        {
            InternalContract.RequireNotNullOrWhiteSpace(serverTechnicalName, nameof(serverTechnicalName));
            serverTechnicalName = serverTechnicalName.ToLower();
            if (_serverTechnicalName != null) InternalContract.Require(serverTechnicalName == _serverTechnicalName,
                $"Once the server name has been set ({_serverTechnicalName}, it can't be changed ({serverTechnicalName}).");
            _serverTechnicalName = serverTechnicalName;
        }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNullOrWhiteSpace(TechnicalMessage, nameof(TechnicalMessage), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(Type, nameof(Type), errorLocation);
            FulcrumValidate.IsNotNullOrWhiteSpace(InstanceId, nameof(InstanceId), errorLocation);
        }

        /// <summary>
        /// Save a key-value to <see cref="Exception.Data"/>
        /// </summary>
        public void SetData<T>(string key, T value)
        {
            // https://stackoverflow.com/questions/65351/null-or-default-comparison-of-generic-argument-in-c-sharp
            if (EqualityComparer<T>.Default.Equals(value, default))
            {
                if (Data.Contains(key)) Data.Remove(key);
            }
            else
            {
                Data[key] = value;
            }
        }

        /// <summary>
        /// Save a key-value to <see cref="Exception.Data"/>
        /// </summary>
        public T GetData<T>(string key)
        {
            if (!Data.Contains(key)) return default;
            var value = (T)Data[key];
            return value;
        }

        /// <inheritdoc />
        public override string StackTrace
        {
            get
            {
                var strings = new StackTrace(this, true)
                    .GetFrames()?
                    .Where(frame => !frame.GetMethod().IsDefined(typeof(StackTraceHiddenAttribute), true))
                    .Select(frame => new StackTrace(frame).ToString())
                    .ToArray();
                return strings != null ? string.Concat(strings) : "";
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{GetType().Name} {TechnicalMessage}";
        }

        /// <inheritdoc />
        public string ToLogString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
