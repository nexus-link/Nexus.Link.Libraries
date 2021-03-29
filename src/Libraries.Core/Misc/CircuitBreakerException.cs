using System;

namespace Nexus.Link.Libraries.Core.Misc
{
    public class CircuitBreakerException : Exception
    {
        public CircuitBreaker CircuitBreaker { get; }

        public CircuitBreakerException(string message, CircuitBreaker circuitBreaker, Exception innerException) : base(message, innerException)
        {
            CircuitBreaker = circuitBreaker;
        }
    }
}