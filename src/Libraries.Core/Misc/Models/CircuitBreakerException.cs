using System;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public class CircuitBreakerException : Exception
    {
        public CircuitBreakerException(Exception innerException) : base("Circuit breaker", innerException) { }
    }
}