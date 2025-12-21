using System;

namespace BpmnWorkflow.Client.Models
{
    public class FormDefinitionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SchemaJson { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpsertFormRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SchemaJson { get; set; } = string.Empty;
    }
}
