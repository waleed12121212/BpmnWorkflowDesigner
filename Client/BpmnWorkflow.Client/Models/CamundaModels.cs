namespace BpmnWorkflow.Client.Models;

// ========== Deployment Models ==========

public class DeployWorkflowRequest
{
    public Guid WorkflowId { get; set; }
    public string DeploymentName { get; set; } = string.Empty;
    public string BpmnXml { get; set; } = string.Empty;
    public bool EnableDuplicateFiltering { get; set; } = true;
    public bool DeployChangedOnly { get; set; } = true;
}

public class DeployWorkflowResponse
{
    public string DeploymentId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public DateTime DeploymentTime { get; set; }
    public string DeploymentName { get; set; } = string.Empty;
}

public class ProcessDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Version { get; set; }
    public string? Category { get; set; }
    public string DeploymentId { get; set; } = string.Empty;
    public bool Suspended { get; set; }
    public string? VersionTag { get; set; }
}

// ========== Process Instance Models ==========

public class StartProcessInstanceRequest
{
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string? BusinessKey { get; set; }
    public Dictionary<string, object>? Variables { get; set; }
}

public class ProcessInstanceDto
{
    public string Id { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string? BusinessKey { get; set; }
    public bool Ended { get; set; }
    public bool Suspended { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    // Display properties
    public string StatusText => Ended ? "Completed" : (Suspended ? "Suspended" : "Running");
    public string StatusColor => Ended ? "success" : (Suspended ? "warning" : "info");
}

// ========== User Task Models ==========

public class UserTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Due { get; set; }
    public DateTime? FollowUp { get; set; }
    public int Priority { get; set; }
    public string TaskDefinitionKey { get; set; } = string.Empty;
    public string? FormKey { get; set; }
    
    // Display properties
    public string PriorityText => Priority switch
    {
        >= 75 => "High",
        >= 50 => "Medium",
        >= 25 => "Low",
        _ => "Very Low"
    };
    
    public string PriorityColor => Priority switch
    {
        >= 75 => "danger",
        >= 50 => "warning",
        >= 25 => "info",
        _ => "secondary"
    };
    
    public bool IsOverdue => Due.HasValue && Due.Value < DateTime.UtcNow;
    public bool IsAssigned => !string.IsNullOrEmpty(Assignee);
}

// ========== Activity Instance Models ==========

public class ActivityInstanceDto
{
    public string Id { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public List<ActivityInstanceDto>? ChildActivityInstances { get; set; }
}

// ========== Statistics Models ==========

public class ProcessStatistics
{
    public int TotalRunning { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalSuspended { get; set; }
    public int TotalTasks { get; set; }
    public int MyTasks { get; set; }
    public int UnassignedTasks { get; set; }
    public int OverdueTasks { get; set; }
}

public class ActivityStatsDto
{
    public string ActivityId { get; set; } = string.Empty;
    public int Instances { get; set; }
    public int Failed { get; set; }
    public int IncidentCount { get; set; }
}

// ========== Variable Editor Models ==========

public class ProcessVariable
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string Type { get; set; } = "String";
    public bool IsEditing { get; set; }
}

// ========== Dashboard Models ==========

public class DashboardStatsDto
{
    public int ActiveInstances { get; set; }
    public int CompletedInstances { get; set; }
    public int TotalInstances { get; set; }
    public int ActiveTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int IncidentCount { get; set; }
    public List<ProcessStatsDto> ProcessStats { get; set; } = new();
    public List<DailyInstanceStatsDto> DailyStats { get; set; } = new();
}

public class ProcessStatsDto
{
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ActiveInstances { get; set; }
    public int IncidentCount { get; set; }
}

public class DailyInstanceStatsDto
{
    public DateTime Date { get; set; }
    public int StartedCount { get; set; }
    public int FinishedCount { get; set; }
}

public class CamundaEnvironmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ZeebeGatewayUrl { get; set; }
    public string? Description { get; set; }
}

public class CamundaEnvironmentUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ZeebeGatewayUrl { get; set; }
    public string? Description { get; set; }
}
