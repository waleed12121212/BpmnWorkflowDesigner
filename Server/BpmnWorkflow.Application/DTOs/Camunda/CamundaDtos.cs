namespace BpmnWorkflow.Application.DTOs.Camunda;

/// <summary>
/// Request to deploy a BPMN workflow to Camunda
/// </summary>
public class DeployWorkflowRequest
{
    public Guid WorkflowId { get; set; }
    public string DeploymentName { get; set; } = string.Empty;
    public string BpmnXml { get; set; } = string.Empty;
    public bool EnableDuplicateFiltering { get; set; } = true;
    public bool DeployChangedOnly { get; set; } = true;
}

/// <summary>
/// Response from Camunda deployment
/// </summary>
public class DeployWorkflowResponse
{
    public string DeploymentId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public DateTime DeploymentTime { get; set; }
    public string DeploymentName { get; set; } = string.Empty;
}

/// <summary>
/// Request to start a process instance
/// </summary>
public class StartProcessInstanceRequest
{
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string? BusinessKey { get; set; }
    public Dictionary<string, object>? Variables { get; set; }
}

/// <summary>
/// Process instance information
/// </summary>
public class ProcessInstanceDto
{
    public string Id { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ProcessDefinitionKey { get; set; } = string.Empty;
    public string ProcessDefinitionName { get; set; } = string.Empty;
    public string? BusinessKey { get; set; }
    public bool Ended { get; set; }
    public bool Suspended { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// User task information
/// </summary>
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
}

/// <summary>
/// Request to complete a user task
/// </summary>
public class CompleteTaskRequest
{
    public Dictionary<string, object>? Variables { get; set; }
}

/// <summary>
/// Process variable
/// </summary>
public class ProcessVariableDto
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string Type { get; set; } = "String";
}

/// <summary>
/// External task information
/// </summary>
public class ExternalTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string WorkerId { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public string ProcessDefinitionId { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public DateTime? LockExpirationTime { get; set; }
    public int Retries { get; set; }
    public Dictionary<string, object>? Variables { get; set; }
}

/// <summary>
/// Request to fetch and lock external tasks
/// </summary>
public class FetchExternalTasksRequest
{
    public string WorkerId { get; set; } = string.Empty;
    public int MaxTasks { get; set; } = 10;
    public bool UsePriority { get; set; } = true;
    public List<TopicSubscription> Topics { get; set; } = new();
}

/// <summary>
/// Topic subscription for external tasks
/// </summary>
public class TopicSubscription
{
    public string TopicName { get; set; } = string.Empty;
    public long LockDuration { get; set; } = 60000; // 1 minute in milliseconds
    public List<string>? Variables { get; set; }
}

/// <summary>
/// Request to complete an external task
/// </summary>
public class CompleteExternalTaskRequest
{
    public string WorkerId { get; set; } = string.Empty;
    public Dictionary<string, object>? Variables { get; set; }
}

/// <summary>
/// Process definition information
/// </summary>
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

/// <summary>
/// Activity instance information
/// </summary>
public class ActivityInstanceDto
{
    public string Id { get; set; } = string.Empty;
    public string ActivityId { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string ProcessInstanceId { get; set; } = string.Empty;
    public List<ActivityInstanceDto>? ChildActivityInstances { get; set; }
}

public class ActivityStatsDto
{
    public string ActivityId { get; set; } = string.Empty;
    public int Instances { get; set; }
    public int Failed { get; set; }
    public int IncidentCount { get; set; }
}

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
    public string? Description { get; set; }
}

public class CamundaEnvironmentUpsertDto
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Description { get; set; }
}
