using BpmnWorkflow.Application.DTOs.Camunda;
using BpmnWorkflow.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BpmnWorkflow.Domain.Entities;

namespace BpmnWorkflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CamundaController : ControllerBase
{
    private readonly ICamundaService _camundaService;
    private readonly IWorkflowService _workflowService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CamundaController> _logger;

    public CamundaController(
        ICamundaService camundaService,
        IWorkflowService workflowService,
        IApplicationDbContext context,
        ILogger<CamundaController> logger)
    {
        _camundaService = camundaService;
        _workflowService = workflowService;
        _context = context;
        _logger = logger;
    }

    // ========== Deployment Endpoints ==========

    /// <summary>
    /// Deploy a workflow to Camunda engine
    /// </summary>
    [HttpPost("deploy/{workflowId}")]
    public async Task<ActionResult<DeployWorkflowResponse>> DeployWorkflow(Guid workflowId)
    {
        try
        {
            // Get workflow from database
            var workflow = await _workflowService.GetWorkflowByIdAsync(workflowId);
            if (workflow == null)
                return NotFound($"Workflow {workflowId} not found");

            if (string.IsNullOrEmpty(workflow.BpmnXml))
                return BadRequest("Workflow does not have BPMN XML content");

            // Deploy to Camunda
            var request = new DeployWorkflowRequest
            {
                WorkflowId = workflowId,
                DeploymentName = string.IsNullOrWhiteSpace(workflow.Name) ? $"Workflow_{workflowId}" : workflow.Name,
                BpmnXml = workflow.BpmnXml,
                EnableDuplicateFiltering = true,
                DeployChangedOnly = true
            };

            var result = await _camundaService.DeployWorkflowAsync(request);

            // Update workflow with Camunda deployment info
            await _workflowService.UpdateCamundaDeploymentInfoAsync(
                workflowId,
                result.DeploymentId,
                result.ProcessDefinitionId
            );

            _logger.LogInformation("Workflow {WorkflowId} deployed to Camunda with deployment ID {DeploymentId}",
                workflowId, result.DeploymentId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error deploying workflow {WorkflowId}", workflowId);
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error to Camunda while deploying workflow {WorkflowId}", workflowId);
            return StatusCode(503, new { error = "Camunda service unreachable", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying workflow {WorkflowId}", workflowId);
            
            if (ex.Message.Contains("Camunda Deployment Error (400)") || ex.Message.Contains("Camunda Deployment Error (404)"))
            {
                return BadRequest(new { error = "Camunda validation or endpoint error", details = ex.Message });
            }
            
            return StatusCode(500, new { error = "Failed to deploy workflow", details = ex.Message });
        }
    }

    /// <summary>
    /// Deploy a DMN to Camunda engine
    /// </summary>
    [HttpPost("deploy-dmn/{dmnId}")]
    public async Task<ActionResult<DeployWorkflowResponse>> DeployDmn(Guid dmnId)
    {
        try
        {
            var dmn = await _context.DmnDefinitions.FirstOrDefaultAsync(d => d.Id == dmnId && !d.IsDeleted);
            if (dmn == null)
                return NotFound($"DMN {dmnId} not found");

            if (string.IsNullOrEmpty(dmn.DmnXml))
                return BadRequest("DMN does not have XML content");

            var result = await _camundaService.DeployDmnAsync(dmnId, dmn.Name, dmn.DmnXml);

            _logger.LogInformation("DMN {DmnId} deployed to Camunda with deployment ID {DeploymentId}",
                dmnId, result.DeploymentId);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error deploying DMN {DmnId}", dmnId);
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying DMN {DmnId}", dmnId);
            return StatusCode(500, new { error = "Failed to deploy DMN", details = ex.Message });
        }
    }

    /// <summary>
    /// Get consolidated statistics for the dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        try
        {
            var stats = await _camundaService.GetDashboardStatsAsync();
            return Ok(stats);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting dashboard statistics");
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard statistics");
            return StatusCode(500, new { error = "Failed to get dashboard statistics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all process definitions from Camunda
    /// </summary>
    [HttpGet("process-definitions")]
    public async Task<ActionResult<List<ProcessDefinitionDto>>> GetProcessDefinitions()
    {
        try
        {
            var definitions = await _camundaService.GetProcessDefinitionsAsync();
            return Ok(definitions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error getting process definitions");
            return StatusCode(503, new { error = "Camunda engine unreachable", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting process definitions");
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definitions");
            return StatusCode(500, new { error = "Failed to get process definitions", details = ex.Message });
        }
    }

    /// <summary>
    /// Get process definition by key
    /// </summary>
    [HttpGet("process-definitions/{key}")]
    public async Task<ActionResult<ProcessDefinitionDto>> GetProcessDefinitionByKey(string key)
    {
        try
        {
            var definition = await _camundaService.GetProcessDefinitionByKeyAsync(key);
            if (definition == null)
                return NotFound($"Process definition with key {key} not found");

            return Ok(definition);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting process definition {Key}", key);
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definition {Key}", key);
            return StatusCode(500, new { error = "Failed to get process definition", details = ex.Message });
        }
    }

    /// <summary>
    /// Get statistics for a process definition
    /// </summary>
    [HttpGet("process-definitions/{id}/statistics")]
    public async Task<ActionResult<List<ActivityStatsDto>>> GetProcessDefinitionStatistics(string id)
    {
        try
        {
            var stats = await _camundaService.GetProcessDefinitionStatisticsAsync(id);
            return Ok(stats);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting statistics for {Id}", id);
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for {Id}", id);
            return StatusCode(500, new { error = "Failed to get statistics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get process definition XML by ID
    /// </summary>
    [HttpGet("process-definitions/{id}/xml")]
    public async Task<ActionResult<string>> GetProcessDefinitionXml(string id)
    {
        try
        {
            var xml = await _camundaService.GetProcessDefinitionXmlAsync(id);
            if (xml == null)
                return NotFound($"Process definition XML with ID {id} not found");

            return Ok(new { bpmn20Xml = xml });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting process definition XML {Id}", id);
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definition XML {Id}", id);
            return StatusCode(500, new { error = "Failed to get process definition XML", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a deployment from Camunda
    /// </summary>
    [HttpDelete("deployments/{deploymentId}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> DeleteDeployment(string deploymentId, [FromQuery] bool cascade = true)
    {
        try
        {
            var success = await _camundaService.DeleteDeploymentAsync(deploymentId, cascade);
            if (!success)
                return NotFound($"Deployment {deploymentId} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting deployment {DeploymentId}", deploymentId);
            return StatusCode(500, new { error = "Failed to delete deployment", details = ex.Message });
        }
    }

    // ========== Process Instance Endpoints ==========

    /// <summary>
    /// Start a new process instance
    /// </summary>
    [HttpPost("processes/start")]
    public async Task<ActionResult<ProcessInstanceDto>> StartProcessInstance([FromBody] StartProcessInstanceRequest request)
    {
        try
        {
            var instance = await _camundaService.StartProcessInstanceAsync(request);
            
            // Sync with local database to track the instance
            try 
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
                {
                    // Find if this definition belongs to one of our workflows
                    // Priority 1: Check by Camunda Process Definition ID
                    var workflow = await _context.Workflows
                        .FirstOrDefaultAsync(w => w.CamundaProcessDefinitionId == instance.ProcessDefinitionId);
                    
                    // Priority 2: Fallback to name/key matches if ID lookup fails
                    if (workflow == null)
                    {
                        workflow = await _context.Workflows
                            .FirstOrDefaultAsync(w => w.Name == request.ProcessDefinitionKey && !w.IsDeleted);
                    }

                    _logger.LogInformation("Syncing process {InstanceId} with local DB. Linked Workflow: {WorkflowId}", 
                        instance.Id, workflow?.Id);

                    var processInstanceEntity = new BpmnWorkflow.Domain.Entities.ProcessInstance
                    {
                        Id = Guid.NewGuid(),
                        WorkflowId = workflow?.Id, // Now nullable
                        CamundaProcessInstanceId = instance.Id,
                        CamundaProcessDefinitionId = instance.ProcessDefinitionId,
                        BusinessKey = instance.BusinessKey,
                        StartedBy = userId,
                        StartedAt = DateTime.UtcNow,
                        State = ProcessInstanceState.Running,
                        IsSuspended = instance.Suspended,
                        Variables = request.Variables != null ? System.Text.Json.JsonSerializer.Serialize(request.Variables) : null
                    };

                    _context.ProcessInstances.Add(processInstanceEntity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning("User ID not found in token claims. Skipping local process instance storage.");
                }
            }
            catch (Exception dbEx)
            {
                // We log but don't fail the request since the process WAS started in Camunda
                _logger.LogWarning(dbEx, "Failed to store process instance {InstanceId} locally, but it was successfully started in Camunda", instance.Id);
            }
            
            _logger.LogInformation("Started process instance {InstanceId} for definition {Key}",
                instance.Id, request.ProcessDefinitionKey);

            return Ok(instance);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error starting process instance");
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting process instance");
            return StatusCode(500, new { error = "Failed to start process instance", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all running process instances
    /// </summary>
    [HttpGet("processes")]
    public async Task<ActionResult<List<ProcessInstanceDto>>> GetProcessInstances([FromQuery] string? processDefinitionKey = null)
    {
        try
        {
            var instances = await _camundaService.GetProcessInstancesAsync(processDefinitionKey);
            return Ok(instances);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error getting process instances");
            return StatusCode(503, new { error = "Camunda engine unreachable", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting process instances");
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instances");
            return StatusCode(500, new { error = "Failed to get process instances", details = ex.Message });
        }
    }

    /// <summary>
    /// Get process instance by ID
    /// </summary>
    [HttpGet("processes/{processInstanceId}")]
    public async Task<ActionResult<ProcessInstanceDto>> GetProcessInstance(string processInstanceId)
    {
        try
        {
            var instance = await _camundaService.GetProcessInstanceAsync(processInstanceId);
            if (instance == null)
                return NotFound($"Process instance {processInstanceId} not found");

            return Ok(instance);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting process instance {ProcessInstanceId}", processInstanceId);
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instance {ProcessInstanceId}", processInstanceId);
            return StatusCode(500, new { error = "Failed to get process instance", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel/Delete a process instance
    /// </summary>
    [HttpDelete("processes/{processInstanceId}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> DeleteProcessInstance(string processInstanceId, [FromQuery] string? reason = null)
    {
        try
        {
            var success = await _camundaService.DeleteProcessInstanceAsync(processInstanceId, reason);
            if (!success)
                return NotFound($"Process instance {processInstanceId} not found");

            // Sync with local database
            try
            {
                var localInstance = await _context.ProcessInstances
                    .FirstOrDefaultAsync(p => p.CamundaProcessInstanceId == processInstanceId);
                
                if (localInstance != null)
                {
                    localInstance.State = ProcessInstanceState.Cancelled;
                    localInstance.EndedAt = DateTime.UtcNow;
                    localInstance.EndReason = reason ?? "Cancelled by user";
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update local process instance {ProcessInstanceId} after cancellation", processInstanceId);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting process instance {ProcessInstanceId}", processInstanceId);
            return StatusCode(500, new { error = "Failed to delete process instance", details = ex.Message });
        }
    }

    /// <summary>
    /// Get historical activities for a process instance
    /// </summary>
    [HttpGet("processes/{processInstanceId}/history-activities")]
    public async Task<ActionResult<List<ActivityInstanceDto>>> GetHistoryActivities(string processInstanceId)
    {
        try
        {
            var activities = await _camundaService.GetHistoryActivitiesAsync(processInstanceId);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history activities for {ProcessInstanceId}", processInstanceId);
            return StatusCode(500, new { error = "Failed to get history activities", details = ex.Message });
        }
    }

    /// <summary>
    /// Get activity instance tree for a process instance
    /// </summary>
    [HttpGet("processes/{processInstanceId}/activities")]
    public async Task<ActionResult<ActivityInstanceDto>> GetActivityInstance(string processInstanceId)
    {
        try
        {
            var activity = await _camundaService.GetActivityInstanceAsync(processInstanceId);
            if (activity == null)
                return NotFound($"Activity instance for process {processInstanceId} not found");

            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity instance for {ProcessInstanceId}", processInstanceId);
            return StatusCode(500, new { error = "Failed to get activity instance", details = ex.Message });
        }
    }


    // ========== User Task Endpoints ==========

    /// <summary>
    /// Get all user tasks
    /// </summary>
    [HttpGet("tasks")]
    public async Task<ActionResult<List<UserTaskDto>>> GetUserTasks(
        [FromQuery] string? assignee = null,
        [FromQuery] string? processInstanceId = null)
    {
        try
        {
            var tasks = await _camundaService.GetUserTasksAsync(assignee, processInstanceId);
            return Ok(tasks);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error getting user tasks");
            return StatusCode(503, new { error = "Camunda engine unreachable", details = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error getting user tasks");
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user tasks");
            return StatusCode(500, new { error = "Failed to get user tasks", details = ex.Message });
        }
    }

    /// <summary>
    /// Get user task by ID
    /// </summary>
    [HttpGet("tasks/{taskId}")]
    public async Task<ActionResult<UserTaskDto>> GetUserTask(string taskId)
    {
        try
        {
            var task = await _camundaService.GetUserTaskAsync(taskId);
            if (task == null)
                return NotFound($"Task {taskId} not found");

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user task {TaskId}", taskId);
            return StatusCode(500, new { error = "Failed to get user task", details = ex.Message });
        }
    }

    /// <summary>
    /// Claim a user task
    /// </summary>
    [HttpPost("tasks/{taskId}/claim")]
    public async Task<IActionResult> ClaimTask(string taskId, [FromBody] ClaimTaskRequest request)
    {
        try
        {
            var success = await _camundaService.ClaimTaskAsync(taskId, request.UserId);
            if (!success)
                return NotFound($"Task {taskId} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming task {TaskId}", taskId);
            return StatusCode(500, new { error = "Failed to claim task", details = ex.Message });
        }
    }

    /// <summary>
    /// Unclaim a user task
    /// </summary>
    [HttpPost("tasks/{taskId}/unclaim")]
    public async Task<IActionResult> UnclaimTask(string taskId)
    {
        try
        {
            var success = await _camundaService.UnclaimTaskAsync(taskId);
            if (!success)
                return NotFound($"Task {taskId} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unclaiming task {TaskId}", taskId);
            return StatusCode(500, new { error = "Failed to unclaim task", details = ex.Message });
        }
    }

    /// <summary>
    /// Complete a user task
    /// </summary>
    [HttpPost("tasks/{taskId}/complete")]
    public async Task<IActionResult> CompleteTask(string taskId, [FromBody] CompleteTaskRequest request)
    {
        try
        {
            var success = await _camundaService.CompleteTaskAsync(taskId, request);
            if (!success)
                return NotFound($"Task {taskId} not found");

            _logger.LogInformation("Task {TaskId} completed", taskId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuration error completing task {TaskId}", taskId);
            return BadRequest(new { error = "Camunda configuration error", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {TaskId}", taskId);
            return StatusCode(500, new { error = "Failed to complete task", details = ex.Message });
        }
    }

    // ========== Process Variables Endpoints ==========

    /// <summary>
    /// Get process instance variables
    /// </summary>
    [HttpGet("processes/{processInstanceId}/variables")]
    public async Task<ActionResult<Dictionary<string, object>>> GetProcessVariables(string processInstanceId)
    {
        try
        {
            var variables = await _camundaService.GetProcessVariablesAsync(processInstanceId);
            return Ok(variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process variables for {ProcessInstanceId}", processInstanceId);
            return StatusCode(500, new { error = "Failed to get process variables", details = ex.Message });
        }
    }

    /// <summary>
    /// Set process instance variables
    /// </summary>
    [HttpPost("processes/{processInstanceId}/variables")]
    public async Task<IActionResult> SetProcessVariables(string processInstanceId, [FromBody] Dictionary<string, object> variables)
    {
        try
        {
            var success = await _camundaService.SetProcessVariablesAsync(processInstanceId, variables);
            if (!success)
                return NotFound($"Process instance {processInstanceId} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting process variables for {ProcessInstanceId}", processInstanceId);
            return StatusCode(500, new { error = "Failed to set process variables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get task variables
    /// </summary>
    [HttpGet("tasks/{taskId}/variables")]
    public async Task<ActionResult<Dictionary<string, object>>> GetTaskVariables(string taskId)
    {
        try
        {
            var variables = await _camundaService.GetTaskVariablesAsync(taskId);
            return Ok(variables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task variables for {TaskId}", taskId);
            return StatusCode(500, new { error = "Failed to get task variables", details = ex.Message });
        }
    }

    // ========== Health Check ==========

    /// <summary>
    /// Check Camunda engine health
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var isHealthy = await _camundaService.IsHealthyAsync();
            if (isHealthy)
                return Ok(new { status = "healthy", message = "Camunda engine is running" });
            
            return StatusCode(503, new { status = "unhealthy", message = "Camunda engine is not responding" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Camunda health");
            return StatusCode(503, new { status = "unhealthy", message = ex.Message });
        }
    }
}

// ========== Request Models ==========

public class ClaimTaskRequest
{
    public string UserId { get; set; } = string.Empty;
}
