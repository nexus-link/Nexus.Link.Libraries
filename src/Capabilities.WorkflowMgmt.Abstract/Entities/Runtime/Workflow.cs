﻿using System.Collections.Generic;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Runtime
{
    public class Workflow
    {
        public WorkflowForm Form { get; set; }
        public WorkflowVersion Version { get; set; }
        public WorkflowInstance Instance { get; set; }

        /// <summary>
        /// Sorted top level activities in position order
        /// </summary>
        public IReadOnlyList<Activity> ActivityTree { get; set; }

        /// <summary>
        /// Sorted top level activities in position order
        /// </summary>
        public IReadOnlyList<Activity> ReferredActivities { get; set; }

        /// <summary>
        /// The activities that haven't been referred yet, i.e. they have no <see cref="Activity.Instance"/>.
        /// </summary>
        public IReadOnlyList<Activity> NotReferredActivities { get; set; }

        /// <inheritdoc />
        public override string ToString() => $"{Form} {Version} {Instance}";
    }
}