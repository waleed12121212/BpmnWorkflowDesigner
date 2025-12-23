using System;

namespace BpmnWorkflow.Domain.Entities
{
    /// <summary>
    /// Represents a running or completed process instance in Camunda
    /// </summary>
    public class ProcessInstance
    {
        public Guid Id { get; set; }
        
        // Link to the workflow definition (optional)
        public Guid? WorkflowId { get; set; }
        public Workflow? Workflow { get; set; }
        
        // Camunda identifiers
        public string CamundaProcessInstanceId { get; set; } = string.Empty;
        public string? CamundaProcessDefinitionId { get; set; }
        public string? BusinessKey { get; set; }
        
        // User who started the process
        public Guid StartedBy { get; set; }
        public User? StartedByUser { get; set; }
        
        // Timestamps
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        
        // State
        public ProcessInstanceState State { get; set; }
        public bool IsSuspended { get; set; }
        
        // Additional metadata
        public string? Variables { get; set; } // JSON serialized variables
        public string? EndReason { get; set; }
    }
    
    /// <summary>
    /// Process instance state enumeration
    /// </summary>
    public enum ProcessInstanceState
    {
        Running = 0,
        Completed = 1,
        Cancelled = 2,
        Failed = 3
    }
}
