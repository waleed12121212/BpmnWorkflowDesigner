using System;

namespace BpmnWorkflow.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid? WorkflowId { get; set; }
        public Workflow? Workflow { get; set; }
        public Guid? FormId { get; set; }
        public FormDefinition? Form { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ElementId { get; set; } // Optional: link comment to a specific element
    }
}
