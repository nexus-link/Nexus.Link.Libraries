using System;
using System.Collections.Generic;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Model;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.Administration
{
    public class Workflow
    {
        /// <summary>
        /// Workflow (instance) id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title of the workflow, including information from Form, Version and Instance
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Timestamp when the workflow (instance) started
        /// </summary>
        public DateTimeOffset StartedAt { get; set; }

        /// <summary>
        /// If not null, timestamp when the workflow (instance) finished
        /// </summary>
        public DateTimeOffset? FinishedAt { get; set; }

        /// <summary>
        /// State of the workflow (instance), calculated from the <see cref="Activities"/>
        /// </summary>
        public ActivityStateEnum State { get; set; } // TODO: Annan enum som sätts på WorkflowInstanceRecord när den ändras

        /// <summary>
        /// Top level activities (instances) of the workflow (instance)
        /// </summary>
        public List<Activity> Activities { get; set; }
    }
}
