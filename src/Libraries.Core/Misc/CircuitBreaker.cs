using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreaker
    {
        private readonly CoolDown _errorCoolDown;
        private StateEnum _state = StateEnum.Ok;

        protected readonly object Lock = new object();
        private Exception _latestException;

        public enum StateEnum
        {
            Ok,
            ContenderIsTrying,
            Failed,
        }


        public DateTimeOffset LastFailAt => _errorCoolDown.LastFailAt;
        public DateTimeOffset? FirstFailureAt { get; private set; }
        public int ConsecutiveErrors { get; private set; }
        public bool ExceptionDueToCircuitBreak => ConsecutiveErrors > 1;

        public CircuitBreaker(CoolDown errorCoolDown)
        {
            _errorCoolDown = errorCoolDown;
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
                        if (!_errorCoolDown.HasCooledDown) return true;
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

        protected virtual void ReportFailure()
        {
            lock (Lock)
            {
                FirstFailureAt = FirstFailureAt ?? DateTimeOffset.UtcNow;
                ConsecutiveErrors++;
                _state = StateEnum.Failed;
                _errorCoolDown.Increase();
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
                _errorCoolDown.Reset();
            }
        }

        public async Task ExecuteOrThrowAsync(Func<Task> action)
        {
            try
            {
                if (IsQuickFailRecommended())
                {
                    FulcrumAssert.IsNotNull(_latestException, CodeLocation.AsString());
                    throw _latestException;
                }
                await action();
                _latestException = null;
                ReportSuccess();
            }
            catch (Exception e)
            {
                _latestException = e;
                ReportFailure();
                throw;
            }
        }
    }
}
