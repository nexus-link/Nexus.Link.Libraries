﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Misc
{
    /// <inheritdoc />
    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly ICoolDownStrategy _errorCoolDownStrategy;
        private StateEnum _state = StateEnum.Ok;

        protected readonly object Lock = new object();
        protected int ConcurrencyCount;

        protected Exception LatestException { get; private set; }

        public enum StateEnum
        {
            Ok,
            ContenderIsTrying,
            Failed,
        }

        /// <inheritdoc />
        public DateTimeOffset LastFailAt => _errorCoolDownStrategy.LastFailAt;

        /// <inheritdoc />
        public virtual bool IsActive => _state != StateEnum.Ok || ConcurrencyCount == 0;

        /// <inheritdoc />
        public DateTimeOffset? FirstFailureAt { get; private set; }

        /// <inheritdoc />
        public int ConsecutiveContenderErrors { get; private set; }

        /// <inheritdoc />
        public bool IsRefusing => _state != StateEnum.Ok;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorCoolDownStrategy">The cool down strategy. This is used to decide when it is time to retry the resource again.</param>
        public CircuitBreaker(ICoolDownStrategy errorCoolDownStrategy)
        {
            _errorCoolDownStrategy = errorCoolDownStrategy;
        }

        protected virtual bool IsQuickFailRecommended()
        {
            lock (Lock)
            {
                switch (_state)
                {
                    case StateEnum.Ok:
                        return false;
                    case StateEnum.Failed:
                        if (!_errorCoolDownStrategy.HasCooledDown) return true;
                        // When the time has come to do another try, we will let the first contender through.
                        _state = StateEnum.ContenderIsTrying;
                        return false;
                    case StateEnum.ContenderIsTrying:
                        // We have a contender. Deny everyone else.
                        return true;
                    default:
                        FulcrumAssert.Fail($"Unknown {typeof(StateEnum).FullName}: {_state}.");
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected virtual void ReportFailure(Exception exception)
        {
            lock (Lock)
            {
                FirstFailureAt = FirstFailureAt ?? DateTimeOffset.UtcNow;
                if (_state == StateEnum.ContenderIsTrying) ConsecutiveContenderErrors++;
                _state = StateEnum.Failed;
                LatestException = exception;
                _errorCoolDownStrategy.Next();
            }
        }

        protected virtual void ReportSuccess()
        {
            if (_state == StateEnum.Ok) return;

            lock (Lock)
            {
                _state = StateEnum.Ok;
                FirstFailureAt = null;
                if (_state == StateEnum.ContenderIsTrying) ConsecutiveContenderErrors = 0;
                LatestException = null;
                _errorCoolDownStrategy.Reset();
            }
        }

        /// <inheritdoc />
        public virtual async Task ExecuteOrThrowAsync(Func<Task> requestAsync)
        {
            await ExecuteOrThrowAsync(async () =>
            {
                await requestAsync();
                return true;
            });
        }

        /// <inheritdoc />
        public virtual async Task<T> ExecuteOrThrowAsync<T>(Func<Task<T>> requestAsync)
        {
            try
            {
                Interlocked.Increment(ref ConcurrencyCount);

                if (IsQuickFailRecommended())
                {
                    FulcrumAssert.IsNotNull(LatestException, CodeLocation.AsString());
                    throw LatestException;
                }

                try
                {
                    var result = await requestAsync();
                    ReportSuccess();
                    return result;
                }
                catch (CircuitBreakerException e)
                {
                    FulcrumAssert.IsNotNull(e.InnerException, CodeLocation.AsString());
                    ReportFailure(e.InnerException);
                    // ReSharper disable once PossibleNullReferenceException
                    throw e.InnerException;
                }
            }
            finally
            {
                Interlocked.Decrement(ref ConcurrencyCount);
            }
        }
    }
}
