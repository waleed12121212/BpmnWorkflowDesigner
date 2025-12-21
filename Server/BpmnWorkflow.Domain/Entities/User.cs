using System;
using System.Collections.Generic;

namespace BpmnWorkflow.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

