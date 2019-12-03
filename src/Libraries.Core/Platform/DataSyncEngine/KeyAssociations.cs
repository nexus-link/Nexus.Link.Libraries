using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Platform.DataSyncEngine
{
    public class KeyAssociations : IValidatable
    {
        public List<SyncKey> AssociatedKeys { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(AssociatedKeys, nameof(AssociatedKeys), errorLocation);
            FulcrumValidate.IsValidated(AssociatedKeys, propertyPath, nameof(AssociatedKeys), errorLocation);
        }
    }
}