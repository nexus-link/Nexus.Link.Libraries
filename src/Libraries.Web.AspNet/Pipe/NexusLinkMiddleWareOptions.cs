#if NETCOREAPP
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    // TODO: Move all features into a class, Feature
    public class NexusLinkMiddlewareOptions : IValidatable
    {
        public MiddlewareFeatures Features = new MiddlewareFeatures();

        public MiddlewareCallbacks Delegates = new MiddlewareCallbacks();
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}
#endif