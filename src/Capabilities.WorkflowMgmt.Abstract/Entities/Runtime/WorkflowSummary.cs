using System.Collections.Generic;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime
{
    public class WorkflowSummary
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
        public IReadOnlyList<ActivitySummary> NotReferredActivities { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Form} {Version} {Instance}";
    }
}