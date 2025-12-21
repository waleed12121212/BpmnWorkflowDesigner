using System;
using System.Collections.Generic;

namespace BpmnWorkflow.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanView { get; set; } = true;
        public bool CanPublish { get; set; }
        public bool CanManageUsers { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}

