using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.Options
{
    public abstract class Feature
    {
        public bool Enabled { get; set; } = false;
    }
}