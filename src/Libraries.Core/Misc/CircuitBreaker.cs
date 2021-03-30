using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreaker
    {
        private readonly CoolDownStrategy _errorCoolDownStrategy;
        private StateEnum _state = StateEnum.Ok;

        protected readonly object Lock = new object();
        protected Exception LatestException { get; private set; }

        public enum StateEnum
        {
            Ok,
            ContenderIsTrying,
            Failed,
        }


        public DateTimeOffset LastFailAt => _errorCoolDownStrategy.LastFailAt;
        public DateTimeOffset? FirstFailureAt { get; private set; }
        public int ConsecutiveErrors { get; private set; }
        public bool ExceptionDueToCircuitBreak => ConsecutiveErrors > 1;

        public CircuitBreaker(CoolDownStrategy errorCoolDownStrategy)
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
                ConsecutiveErrors++;
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
                ConsecutiveErrors = 0;
                LatestException = null;
                _errorCoolDownStrategy.Reset();
            }
        }

        public virtual async Task ExecuteOrThrowAsync(Func<Task> action)
        {
            try
            {
                if (IsQuickFailRecommended())
                {
                    FulcrumAssert.IsNotNull(LatestException, CodeLocation.AsString());
                    throw LatestException;
                }
                await action();
                ReportSuccess();
            }
            catch (Exception e)
            {
                ReportFailure(e);
                throw;
            }
        }
    }
}
