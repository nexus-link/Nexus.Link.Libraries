using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreakerCollection
    {
        private readonly Dictionary<string, ICircuitBreaker> _circuitBreakers = new Dictionary<string, ICircuitBreaker>();
        private readonly Func<ICircuitBreaker> _createCircuitBreakerDelegate;

        public CircuitBreakerCollection(Func<ICircuitBreaker> createCircuitBreakerDelegate)
        {
            _createCircuitBreakerDelegate = createCircuitBreakerDelegate;
        }

        public bool TryGet(string key, out ICircuitBreaker circuitBreaker)
        {
            lock (_circuitBreakers)
            {
                return _circuitBreakers.TryGetValue(key, out circuitBreaker);
            }
        }

        public async Task ExecuteOrThrowAsync(string key, Func<CancellationToken, Task> actionAsync, CancellationToken cancellationToken = default)
        {
            await ExecuteOrThrowAsync<bool>(key, async (t) =>
            {
                await actionAsync(t);
                return true;
            }, cancellationToken);
        }

        public async Task<T> ExecuteOrThrowAsync<T>(string key, Func<CancellationToken, Task<T>> requestAsync, CancellationToken cancellationToken = default)
        {
            ICircuitBreaker circuitBreaker;
            lock (_circuitBreakers)
            {
                if (!_circuitBreakers.TryGetValue(key, out circuitBreaker))
                {
                    circuitBreaker = _createCircuitBreakerDelegate();
                    _circuitBreakers.Add(key, circuitBreaker);
                }
            }
            try
            {
                return await circuitBreaker.ExecuteOrThrowAsync(requestAsync, cancellationToken);
            }
            finally
            {
                lock (_circuitBreakers)
                {
                    if (!circuitBreaker.IsActive)
                    {
                        _circuitBreakers.Remove(key);
                    }
                }
            }
        }

        public void ResetCollection()
        {
            lock (_circuitBreakers)
            {
                _circuitBreakers.Clear();
            }
        }
    }
}