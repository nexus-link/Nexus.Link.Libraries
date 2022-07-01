#if NETCOREAPP
using System;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Support.Options;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe
{
    // TODO: Move all features into a class, Feature
    [Obsolete("Please use Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddlewareOptions. Obsolete since 2022-04-11.")]
    public class NexusLinkMiddlewareOptions : IValidatable
    {
        [Obsolete("Please use Nexus.Link.Misc.AspNet.Sdk.Inbound.NexusLinkMiddlewareOptions.Features. Obsolete since 2022-04-11.")]
        public MiddlewareFeatures Features = new MiddlewareFeatures();
        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}
#endif