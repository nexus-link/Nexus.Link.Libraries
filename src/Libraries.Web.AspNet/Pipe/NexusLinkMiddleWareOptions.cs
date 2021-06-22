#if NETCOREAPP
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Support.Options;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    // TODO: Move all features into a class, Feature
    public class NexusLinkMiddlewareOptions : IValidatable
    {
        public MiddlewareFeatures Features = new MiddlewareFeatures();
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}
#endif