using System;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public class ChokeException : Exception
    {
        public ChokeException(Exception innerException) : base("Choke", innerException)
        {
            InternalContract.RequireNotNull(innerException, nameof(innerException));
        }
    }
}