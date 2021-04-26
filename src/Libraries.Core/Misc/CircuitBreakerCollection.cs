using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Libraries.Core.Misc
{
    /// <inheritdoc />
    public class CircuitBreakerCollection : ICircuitBreakerCollection
    {
        private readonly Dictionary<string, ICircuitBreaker> _circuitBreakers = new Dictionary<string, ICircuitBreaker>();
        private readonly Func<ICircuitBreaker> _createCircuitBreakerDelegate;

        public CircuitBreakerCollection(Func<ICircuitBreaker> createCircuitBreakerDelegate)
        {
            _createCircuitBreakerDelegate = createCircuitBreakerDelegate;
        }

        /// <inheritdoc />
        public bool TryGet(string key, out ICircuitBreaker circuitBreaker)
        {
            lock (_circuitBreakers)
            {
                return _circuitBreakers.TryGetValue(key, out circuitBreaker);
            }
        }



        /// <inheritdoc />
        public async Task ExecuteOrThrowAsync(string key, Func<CancellationToken, Task> requestAsync, CancellationToken cancellationToken = default)
        {
            await ExecuteOrThrowAsync<bool>(key, async (t) =>
            {
                await requestAsync(t);
                return true;
            }, cancellationToken);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void ForceEndOfCoolDown()
        {
            lock (_circuitBreakers)
            {
                foreach (var circuitBreaker in _circuitBreakers.Values)
                {
                    circuitBreaker.ForceEndOfCoolDown();
                }
            }
        }

        /// <inheritdoc />
        public void ForceEndOfCoolDown(string key)
        {
            lock (_circuitBreakers)
            {
                if (!_circuitBreakers.TryGetValue(key, out var circuitBreaker)) return;
                circuitBreaker.ForceEndOfCoolDown();
            }
        }

        /// <inheritdoc />
        public void ResetCollection()
        {
            lock (_circuitBreakers)
            {
                _circuitBreakers.Clear();
            }
        }
    }
}