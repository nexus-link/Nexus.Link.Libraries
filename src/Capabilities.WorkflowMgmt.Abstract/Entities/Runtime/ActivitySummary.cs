using System.Collections.Generic;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime
{
    public class ActivitySummary
    {
        public ActivityForm Form { get; set; }
        public ActivityVersion Version { get; set; }
        public ActivityInstance Instance { get; set; }

        /// <summary>
        /// Sorted child activities in position order
        /// </summary>
        public IReadOnlyList<ActivitySummary> Children { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Form} {Version} {Instance}";
    }
}