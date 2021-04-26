using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    /// <summary>
    /// Handles a collection of <see cref="ICircuitBreaker"/>.
    /// </summary>
    public interface ICircuitBreakerCollection
    {
        /// <summary>
        /// The main method for the circuit breaker. In the <paramref name="requestAsync"/> method, if you experience
        /// an exception that you think should trigger the circuit breaker, you you should throw a
        /// <see cref="CircuitBreakerException"/> with the actual exception as it's inner exception. This will break the
        /// circuit.
        /// </summary>
        /// <param name="key">The key to pick a specific circuit breaker for this request.</param>
        /// <param name="requestAsync">The request that should be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="CircuitBreakerException">When you experience an exception that you think should trigger the circuit breaker, then
        /// throw a <see cref="CircuitBreakerException"/> with your original exception as the inner exception. This will trigger the circuit breaker to
        /// break the circuit and finally throw the inner exception. The inner exception will be rethrown for every consecutive call to this method
        /// until eventually the circuit breaker will let a call pass through to check if the resource is OK.</exception>
        Task ExecuteOrThrowAsync(string key, Func<CancellationToken, Task> requestAsync, CancellationToken cancellationToken = default);

        /// <summary>
        /// The main method for the circuit breaker. In the <paramref name="requestAsync"/> method, if you experience
        /// an exception that you think should trigger the circuit breaker, you you should throw a
        /// <see cref="CircuitBreakerException"/> with the actual exception as it's inner exception. This will break the
        /// circuit.
        /// </summary>
        /// <param name="key">The key to pick a specific circuit breaker for this request.</param>
        /// <param name="requestAsync">The request that should be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="CircuitBreakerException">When you experience an exception that you think should trigger the circuit breaker, then
        /// throw a <see cref="CircuitBreakerException"/> with your original exception as the inner exception. This will trigger the circuit breaker to
        /// break the circuit and finally throw the inner exception. The inner exception will be rethrown for every consecutive call to this method
        /// until eventually the circuit breaker will let a call pass through to check if the resource is OK.</exception>
        Task<T> ExecuteOrThrowAsync<T>(string key, Func<CancellationToken, Task<T>> requestAsync, CancellationToken cancellationToken = default);

        /// <summary>
        /// Override the cool down period for a specific <see cref="ICircuitBreaker"/>, meaning HasCooledDown == true immediately
        /// </summary>
        /// <param name="key">The key for the <see cref="ICircuitBreaker"/> that we want to break the cool down period for.</param>
        void ForceEndOfCoolDown(string key);

        /// <summary>
        /// Override the cool down period for all <see cref="ICircuitBreaker"/>, meaning HasCooledDown == true immediately
        /// </summary>
        void ForceEndOfCoolDown();

        /// <summary>
        /// Forget about all previous circuit breakers; start with a clean slate.
        /// </summary>
        void ResetCollection();

        /// <summary>
        /// Get a the <paramref name="circuitBreaker"/> that was pointed out by <paramref name="key"/>.
        /// </summary>
        /// <returns></returns>
        bool TryGet(string key, out ICircuitBreaker circuitBreaker);
    }
}