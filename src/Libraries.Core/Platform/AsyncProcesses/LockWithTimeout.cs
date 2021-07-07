using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Platform.AsyncProcesses
{
    public class LockWithTimeout
    {
        private readonly object _internalObjectLock = new object();
        private static readonly TimeSpan DefaultLockTimeSpan = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan TimeSpanBetweenRetries = TimeSpan.FromMilliseconds(10);
        public Guid? Id { get; private set; }

        public DateTimeOffset? LockedUntil { get; private set; }

        public bool TryLock(TimeSpan lockTimeSpan, out Guid lockId)
        {
            lockId = default;
            lock (_internalObjectLock)
            {
                if (IsLocked()) return false;
                LockedUntil = DateTimeOffset.UtcNow + lockTimeSpan;
                lockId = Guid.NewGuid();
                Id = lockId;
                return true;
            }
        }

        public void Unlock(Guid lockId)
        {
            lock (_internalObjectLock)
            {
                if (Id == null || Id != lockId) return;
                Id = null;
                LockedUntil = null;
            }
        }

        public bool IsLocked()
        {
            return LockedUntil.HasValue && LockedUntil < DateTimeOffset.UtcNow;
        }

        public async Task<Guid> WaitForLockAsync(TimeSpan? lockTimeSpan = null, TimeSpan? maxWaitTimeSpan= null)
        {
            lockTimeSpan = lockTimeSpan ?? DefaultLockTimeSpan;
            maxWaitTimeSpan = maxWaitTimeSpan ?? lockTimeSpan.Value;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                if (TryLock(lockTimeSpan.Value, out var lockId)) return lockId;
                if (stopwatch.Elapsed > maxWaitTimeSpan) throw new PostponeException();
                await Task.Delay(TimeSpanBetweenRetries);
            }
        }

        public LockWithTimeout AsCopy()
        {
            return new LockWithTimeout
            {
                Id = Id,
                LockedUntil = LockedUntil
            };
        }
    }
}