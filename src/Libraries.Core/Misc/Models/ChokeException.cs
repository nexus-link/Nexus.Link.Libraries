using System;

namespace Nexus.Link.Libraries.Core.Misc.Models
{
    public class ChokeException : Exception
    {
        public ChokeException(Exception innerException) : base("Choke", innerException){}
    }
}