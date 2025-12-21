using System;
using System.Collections.Generic;

namespace BpmnWorkflow.Domain.Entities
{
    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

