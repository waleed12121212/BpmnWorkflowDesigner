using BpmnWorkflow.Application.DTOs.Camunda;

namespace BpmnWorkflow.Application.Interfaces;

/// <summary>
/// Service interface for Camunda Platform 7 integration
/// </summary>
public interface ICamundaService
{
    // ========== Deployment Operations ==========
    
    /// <summary>
    /// Deploy a BPMN workflow to Camunda engine
    /// </summary>
    Task<DeployWorkflowResponse> DeployWorkflowAsync(DeployWorkflowRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deploy a DMN decision table to Camunda engine
    /// </summary>
    Task<DeployWorkflowResponse> DeployDmnAsync(Guid dmnId, string name, string xml, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all process definitions
    /// </summary>
    Task<List<ProcessDefinitionDto>> GetProcessDefinitionsAsync(Guid? environmentId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get process definition by key
    /// </summary>
    Task<ProcessDefinitionDto?> GetProcessDefinitionByKeyAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a deployment
    /// </summary>
    Task<bool> DeleteDeploymentAsync(string deploymentId, bool cascade = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the BPMN XML of a process definition
    /// </summary>
    Task<string?> GetProcessDefinitionXmlAsync(string processDefinitionId, CancellationToken cancellationToken = default);
    
    // ========== Process Instance Operations ==========
    
    /// <summary>
    /// Get historical activity instances
    /// </summary>
    Task<List<ActivityInstanceDto>> GetHistoryActivitiesAsync(string processInstanceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get statistics for a process definition (e.g., activity execution counts)
    /// </summary>
    Task<List<ActivityStatsDto>> GetProcessDefinitionStatisticsAsync(string processDefinitionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Start a new process instance
    /// </summary>
    Task<ProcessInstanceDto> StartProcessInstanceAsync(StartProcessInstanceRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get process instance by ID
    /// </summary>
    Task<ProcessInstanceDto?> GetProcessInstanceAsync(string processInstanceId, Guid? environmentId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all active process instances
    /// </summary>
    Task<List<ProcessInstanceDto>> GetProcessInstancesAsync(string? processDefinitionKey = null, Guid? environmentId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Cancel/Delete a process instance
    /// </summary>
    Task<bool> DeleteProcessInstanceAsync(string processInstanceId, string? reason = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get activity instance tree for a process instance
    /// </summary>
    Task<ActivityInstanceDto?> GetActivityInstanceAsync(string processInstanceId, CancellationToken cancellationToken = default);
    
    // ========== User Task Operations ==========
    
    /// <summary>
    /// Get all user tasks
    /// </summary>
    Task<List<UserTaskDto>> GetUserTasksAsync(string? assignee = null, string? processInstanceId = null, Guid? environmentId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user task by ID
    /// </summary>
    Task<UserTaskDto?> GetUserTaskAsync(string taskId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Claim a user task
    /// </summary>
    Task<bool> ClaimTaskAsync(string taskId, string userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Unclaim a user task
    /// </summary>
    Task<bool> UnclaimTaskAsync(string taskId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Complete a user task
    /// </summary>
    Task<bool> CompleteTaskAsync(string taskId, CompleteTaskRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default);
    
    // ========== Process Variables Operations ==========
    
    /// <summary>
    /// Get process instance variables
    /// </summary>
    Task<Dictionary<string, object>> GetProcessVariablesAsync(string processInstanceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set process instance variables
    /// </summary>
    Task<bool> SetProcessVariablesAsync(string processInstanceId, Dictionary<string, object> variables, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get task variables
    /// </summary>
    Task<Dictionary<string, object>> GetTaskVariablesAsync(string taskId, CancellationToken cancellationToken = default);
    
    // ========== External Task Operations ==========
    
    /// <summary>
    /// Fetch and lock external tasks
    /// </summary>
    Task<List<ExternalTaskDto>> FetchAndLockExternalTasksAsync(FetchExternalTasksRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Complete an external task
    /// </summary>
    Task<bool> CompleteExternalTaskAsync(string taskId, CompleteExternalTaskRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Report external task failure
    /// </summary>
    Task<bool> ReportExternalTaskFailureAsync(string taskId, string workerId, string errorMessage, int retries = 3, CancellationToken cancellationToken = default);
    
    // ========== Health Check ==========
    
    /// <summary>
    /// Get consolidated statistics for the dashboard
    /// </summary>
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? environmentId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if Camunda engine is available
    /// </summary>
    Task<bool> IsHealthyAsync(Guid? environmentId = null, CancellationToken cancellationToken = default);
}
