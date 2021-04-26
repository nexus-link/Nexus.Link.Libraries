using System;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public class CircuitBreakerException : Exception
    {
        public CircuitBreakerException(Exception innerException) : base("Circuit breaker", innerException)
        {
            InternalContract.RequireNotNull(innerException, nameof(innerException));
        }
    }
}