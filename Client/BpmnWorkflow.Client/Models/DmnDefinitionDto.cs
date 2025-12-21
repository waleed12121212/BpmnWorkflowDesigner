using System;

namespace BpmnWorkflow.Client.Models
{
    public class DmnDefinitionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DmnXml { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpsertDmnRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DmnXml { get; set; } = string.Empty;
    }

    public class DmnVersionDto
    {
        public Guid Id { get; set; }
        public Guid DmnId { get; set; }
        public string DmnXml { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Comment { get; set; }
    }
}
