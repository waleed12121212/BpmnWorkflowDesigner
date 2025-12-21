using System;

namespace BpmnWorkflow.Client.Models
{
    public class WorkflowVersionDto
    {
        public Guid Id { get; set; }
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Comment { get; set; }
        public string? BpmnXml { get; set; }
    }
}
