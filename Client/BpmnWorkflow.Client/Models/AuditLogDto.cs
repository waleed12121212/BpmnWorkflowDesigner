using System;

namespace BpmnWorkflow.Client.Models
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string? Changes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
