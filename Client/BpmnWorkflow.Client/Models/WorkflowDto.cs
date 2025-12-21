using System;

namespace BpmnWorkflow.Client.Models
{
    public class WorkflowDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPublished { get; set; }
        public string? BpmnXml { get; set; }
        public string? SvgPreview { get; set; }
        public int Version { get; set; }
    }
}


