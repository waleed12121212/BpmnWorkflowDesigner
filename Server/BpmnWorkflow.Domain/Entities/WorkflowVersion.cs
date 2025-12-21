using System;

namespace BpmnWorkflow.Domain.Entities
{
    public class WorkflowVersion
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public Workflow? Workflow { get; set; }
        public string BpmnXml { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Comment { get; set; }
    }
}
