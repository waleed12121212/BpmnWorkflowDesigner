using System;

namespace BpmnWorkflow.Domain.Entities
{
    public class FormVersion
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public FormDefinition? Form { get; set; }
        public string SchemaJson { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? Comment { get; set; }
    }
}
