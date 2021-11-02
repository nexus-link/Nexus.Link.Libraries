using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Configuration;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State
{
    public class WorkflowSummary : IValidatable
    {
        public WorkflowForm Form { get; set; }
        public WorkflowVersion Version { get; set; }
        public WorkflowInstance Instance { get; set; }

        /// <summary>
        /// Sorted top level activities in position order
        /// </summary>
        public IReadOnlyList<ActivitySummary> ActivityTree { get; set; }

        /// <summary>
        /// Sorted top level activities in position order
        /// </summary>
        public IReadOnlyList<ActivitySummary> ReferredActivities { get; set; }

        /// <summary>
        /// The activities that haven't been referred yet, i.e. they have no <see cref="ActivitySummary.Instance"/>.
        /// </summary>
        public IDictionary<string, ActivityForm> ActivityForms{ get; set; }
        public IDictionary<string, ActivityVersion> ActivityVersions { get; set; }
        public IDictionary<string, ActivityInstance> ActivityInstances{ get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Form} {Version} {Instance}";

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Instance != null)
            {
                FulcrumValidate.IsNotNull(Version, nameof(Version), errorLocation);
            }
            if (Version != null)
            {
                FulcrumValidate.IsNotNull(Form, nameof(Form), errorLocation);
            }
            
            FulcrumValidate.IsNotNull(ActivityTree, nameof(ActivityTree), errorLocation);
            FulcrumValidate.IsNotNull(ReferredActivities, nameof(ReferredActivities), errorLocation);
            FulcrumValidate.IsNotNull(ActivityForms, nameof(ActivityForms), errorLocation);
            FulcrumValidate.IsNotNull(ActivityVersions, nameof(ActivityVersions), errorLocation);
            FulcrumValidate.IsNotNull(ActivityInstances, nameof(ActivityInstances), errorLocation);
        }
    }
}