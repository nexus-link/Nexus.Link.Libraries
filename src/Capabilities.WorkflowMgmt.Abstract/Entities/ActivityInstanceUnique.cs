namespace Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities
{
    public class ActivityInstanceUnique
    {
        public string WorkflowInstanceId { get; set; }
        public string ActivityVersionId { get; set; }
        public string ParentActivityInstanceId { get; set; }
        public int? Iteration { get; set; }
    }
}