using System.Collections.Generic;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime
{
    public class Activity
    {
        public ActivityForm Form { get; set; }
        public ActivityVersion Version { get; set; }
        public ActivityInstance Instance { get; set; }

        /// <summary>
        /// Sorted child activities in position order
        /// </summary>
        public List<Activity> Children { get; set; }
    }
}