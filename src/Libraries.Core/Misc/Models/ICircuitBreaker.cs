using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    /// <summary>
    /// The interface for a circuit breaker.
    /// Use this when you send requests to a resource that you might want to stop using temporarily when it malfunctions. When you
    /// tell the circuit breaker that the resource is malfunctioning, it will no longer call the resource and rethrow the last exception,
    /// until it eventually tries the resource to see if it is working again.
    /// </summary>
    /// <remarks>
    /// See https://medium.com/bonniernewstech/circuit-breaker-pattern-what-and-why-a17f8babbec0 for more details.
    /// </remarks>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// The number of consecutive retries that has resulted in errors.
        /// </summary>
        int ConsecutiveContenderErrors { get; }

        /// <summary>
        /// True if the circuit breaker is in an "open" state, i.e. actively refuses to execute the requests.
        /// </summary>
        bool IsRefusing { get; }

        /// <summary>
        /// The time when the last fail occurred.
        /// </summary>
        DateTimeOffset LastFailAt { get; }

        /// <summary>
        /// The time when the first fail occurred, where no success has been made since.
        /// </summary>
        DateTimeOffset? FirstFailureAt { get; }

        /// <summary>
        /// True if the circuit breaker is either in an "open" state or if it is executing at least one request currently.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// The main method for the circuit breaker. In the <paramref name="requestAsync"/> method, if you experience
        /// an exception that you think should trigger the circuit breaker, you you should throw a
        /// <see cref="CircuitBreakerException"/> with the actual exception as it's inner exception. This will break the
        /// circuit.
        /// </summary>
        /// <param name="requestAsync">The request that should be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="CircuitBreakerException">When you experience an exception that you think should trigger the circuit breaker, then
        /// throw a <see cref="CircuitBreakerException"/> with your original exception as the inner exception. This will trigger the circuit breaker to
        /// break the circuit and finally throw the inner exception. The inner exception will be rethrown for every consecutive call to this method
        /// until eventually the circuit breaker will let a call pass through to check if the resource is OK.</exception>
        Task ExecuteOrThrowAsync(Func<CancellationToken, Task> requestAsync, CancellationToken cancellationToken = default);

        /// <summary>
        /// The main method for the circuit breaker. In the <paramref name="requestAsync"/> method, if you experience
        /// an exception that you think should trigger the circuit breaker, you you should throw a
        /// <see cref="CircuitBreakerException"/> with the actual exception as it's inner exception. This will break the
        /// circuit.
        /// </summary>
        /// <param name="requestAsync">The request that should be executed.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="CircuitBreakerException">When you experience an exception that you think should trigger the circuit breaker, then
        /// throw a <see cref="CircuitBreakerException"/> with your original exception as the inner exception. This will trigger the circuit breaker to
        /// break the circuit and finally throw the inner exception. The inner exception will be rethrown for every consecutive call to this method
        /// until eventually the circuit breaker will let a call pass through to check if the resource is OK.</exception>
        Task<T> ExecuteOrThrowAsync<T>(Func<CancellationToken, Task<T>> requestAsync, CancellationToken cancellationToken = default);
    }
}