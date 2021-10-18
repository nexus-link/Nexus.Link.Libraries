using System;
using System.Collections.Generic;
using System.Text;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Services;

namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract
{
    public interface IAdministrationCapability
    {
        IWorkflowAdministrationService WorkflowAdministrationService {  get; }
    }
}
