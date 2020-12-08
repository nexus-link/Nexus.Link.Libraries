using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Platform.DataSyncEngine
{
    public class KeyAssociations : IValidatable
    {
        /// <summary>
        /// The key that logging etc will be based upon
        /// </summary>
        public SyncKey Key { get; set; }

        /// <summary>
        /// Set to True if the Main Key (<see cref="Key"/>) should be the winner in a merge, set to 
        /// False for normal winning/loosing logic in DSE.
        /// </summary>
        public bool MainKeyAsMergeWinner { get; set; }

        /// <summary>
        /// Keys associated with <see cref="Key"/>, pointing to the same object
        /// </summary>
        public List<SyncKey> AssociatedKeys { get; set; }

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(Key, nameof(Key), errorLocation);
            FulcrumValidate.IsValidated(Key, propertyPath, nameof(Key), errorLocation);
            FulcrumValidate.IsNotNull(AssociatedKeys, nameof(AssociatedKeys), errorLocation);
            FulcrumValidate.IsValidated(AssociatedKeys, propertyPath, nameof(AssociatedKeys), errorLocation);
        }
    }
}