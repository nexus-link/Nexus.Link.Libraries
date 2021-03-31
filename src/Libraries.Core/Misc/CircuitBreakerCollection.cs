using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public async Task ExecuteOrThrowAsync(string key, Func<Task> actionAsync)
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
                await circuitBreaker.ExecuteOrThrowAsync(actionAsync);
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
    }
}