using System;
using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;

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


        public ActivityStateEnum State { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// State of the activity (instance)
        /// </summary>
        public AnnotatedWorkflowId WaitingForWorkflow { get; set; }

        /// <summary>
        /// Sub activities (instances) of this activity (instance)
        /// </summary>
        public List<Activity> Children { get; set; }
    }
}