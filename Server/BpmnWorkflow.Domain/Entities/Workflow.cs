using System;
using System.Collections.Generic;

namespace BpmnWorkflow.Domain.Entities
{
    public class Workflow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BpmnXml { get; set; } = string.Empty;
        public string? SvgPreview { get; set; }
        public Guid OwnerId { get; set; }
        public User? Owner { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public int Version { get; set; }
        public bool IsPublished { get; set; }
        public string? Tags { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

