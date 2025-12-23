using System;
using System.ComponentModel.DataAnnotations;

namespace BpmnWorkflow.Domain.Entities
{
    public class CamundaEnvironment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Url]
        public string BaseUrl { get; set; } = string.Empty;

        public string? Username { get; set; }
        public string? Password { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
    }
}
