using System;

namespace BpmnWorkflow.Domain.Entities
{
    public class FormDefinition
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SchemaJson { get; set; } = "{\"components\": [], \"type\": \"default\"}";
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
