using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Misc.Models;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration
{
    public class Activity
    {
        /// <summary>
        /// Activity (instance) id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title of the activity, including information from Form
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The nested position of the activity
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// Timestamp when the activity (instance) started
        /// </summary>
        public DateTimeOffset StartedAt { get; set; }

        /// <summary>
        /// If not null, timestamp when the activity (instance) finished
        /// </summary>
        public DateTimeOffset? FinishedAt { get; set; }

        /// <summary>
        /// State of the activity (instance)
        /// </summary>
        public ActivityStateEnum State { get; set; }

        /// <summary>
        /// An error message that is readable by business people
        /// </summary>
        public string FriendlyErrorMessage { get; set; }

        /// <summary>
        /// An error message that can contain technical details
        /// </summary>
        public string TechnicalErrorMessage { get; set; }

        /// <summary>
        /// Reference to a workflow that this activity waits for
        /// </summary>
        public AnnotatedId<string> WaitingForWorkflow { get; set; }

        /// <summary>
        /// Sub activities (instances) of this activity (instance)
        /// </summary>
        public List<Activity> Children { get; set; }
    }
}