using BpmnWorkflow.Client.Models;
using System.Net.Http.Json;

namespace BpmnWorkflow.Client.Services;

/// <summary>
/// Client service for Camunda integration
/// </summary>
public class CamundaClientService
{
    private readonly HttpClient _httpClient;

    public CamundaClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // ========== Deployment Operations ==========

    public async Task<DeployWorkflowResponse?> DeployWorkflowAsync(Guid workflowId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/camunda/deploy/{workflowId}", null);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = "Deployment failed.";
                try
                {
                    var error = await response.Content.ReadFromJsonAsync<dynamic>();
                    if (error != null)
                    {
                        errorMsg = error.ToString();
                        // Try to extract 'details' or 'error' from the dynamic object if they exist
                    }
                }
                catch
                {
                    var details = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(details)) errorMsg = details;
                }
                
                throw new Exception(errorMsg);
            }
            
            return await response.Content.ReadFromJsonAsync<DeployWorkflowResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deploying workflow: {ex.Message}");
            throw;
        }
    }

    public async Task<DeployWorkflowResponse?> DeployDmnAsync(Guid dmnId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/camunda/deploy-dmn/{dmnId}", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DeployWorkflowResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deploying DMN: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ProcessDefinitionDto>> GetProcessDefinitionsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ProcessDefinitionDto>>("api/camunda/process-definitions") 
                   ?? new List<ProcessDefinitionDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting process definitions: {ex.Message}");
            return new List<ProcessDefinitionDto>();
        }
    }

    public async Task<List<ActivityStatsDto>> GetProcessDefinitionStatisticsAsync(string id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ActivityStatsDto>>($"api/camunda/process-definitions/{id}/statistics") 
                   ?? new List<ActivityStatsDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting statistics: {ex.Message}");
            return new List<ActivityStatsDto>();
        }
    }

    public async Task<string?> GetProcessDefinitionXmlAsync(string processDefinitionId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<CamundaXmlWrapper>($"api/camunda/process-definitions/{processDefinitionId}/xml");
            return result?.Bpmn20Xml;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting process definition XML: {ex.Message}");
            return null;
        }
    }

    private class CamundaXmlWrapper
    {
        public string Bpmn20Xml { get; set; } = string.Empty;
    }

    public async Task<bool> DeleteDeploymentAsync(string deploymentId, bool cascade = true)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/camunda/deployments/{deploymentId}?cascade={cascade}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting deployment: {ex.Message}");
            return false;
        }
    }

    // ========== Process Instance Operations ==========

    public async Task<ProcessInstanceDto?> StartProcessInstanceAsync(StartProcessInstanceRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/camunda/processes/start", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProcessInstanceDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting process instance: {ex.Message}");
            throw;
        }
    }

    public async Task<List<ProcessInstanceDto>> GetProcessInstancesAsync(string? processDefinitionKey = null)
    {
        try
        {
            var url = "api/camunda/processes";
            if (!string.IsNullOrEmpty(processDefinitionKey))
                url += $"?processDefinitionKey={processDefinitionKey}";

            return await _httpClient.GetFromJsonAsync<List<ProcessInstanceDto>>(url) 
                   ?? new List<ProcessInstanceDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting process instances: {ex.Message}");
            return new List<ProcessInstanceDto>();
        }
    }

    public async Task<ProcessInstanceDto?> GetProcessInstanceAsync(string processInstanceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProcessInstanceDto>($"api/camunda/processes/{processInstanceId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting process instance: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteProcessInstanceAsync(string processInstanceId, string? reason = null)
    {
        try
        {
            var url = $"api/camunda/processes/{processInstanceId}";
            if (!string.IsNullOrEmpty(reason))
                url += $"?reason={Uri.EscapeDataString(reason)}";

            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting process instance: {ex.Message}");
            return false;
        }
    }

    public async Task<ActivityInstanceDto?> GetActivityInstanceAsync(string processInstanceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ActivityInstanceDto>($"api/camunda/processes/{processInstanceId}/activities");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting activity instance: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ActivityInstanceDto>> GetHistoryActivitiesAsync(string processInstanceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ActivityInstanceDto>>($"api/camunda/processes/{processInstanceId}/history-activities") 
                   ?? new List<ActivityInstanceDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting history activities: {ex.Message}");
            return new List<ActivityInstanceDto>();
        }
    }

    // ========== User Task Operations ==========

    public async Task<List<UserTaskDto>> GetUserTasksAsync(string? assignee = null, string? processInstanceId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(assignee))
                queryParams.Add($"assignee={assignee}");
            if (!string.IsNullOrEmpty(processInstanceId))
                queryParams.Add($"processInstanceId={processInstanceId}");

            var url = "api/camunda/tasks";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            return await _httpClient.GetFromJsonAsync<List<UserTaskDto>>(url) 
                   ?? new List<UserTaskDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user tasks: {ex.Message}");
            return new List<UserTaskDto>();
        }
    }

    public async Task<UserTaskDto?> GetUserTaskAsync(string taskId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserTaskDto>($"api/camunda/tasks/{taskId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting user task: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ClaimTaskAsync(string taskId, string userId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/camunda/tasks/{taskId}/claim", new { userId });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error claiming task: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UnclaimTaskAsync(string taskId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/camunda/tasks/{taskId}/unclaim", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unclaiming task: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CompleteTaskAsync(string taskId, Dictionary<string, object>? variables = null)
    {
        try
        {
            var request = new { variables };
            var response = await _httpClient.PostAsJsonAsync($"api/camunda/tasks/{taskId}/complete", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error completing task: {ex.Message}");
            return false;
        }
    }

    // ========== Process Variables Operations ==========

    public async Task<Dictionary<string, object>> GetProcessVariablesAsync(string processInstanceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, object>>($"api/camunda/processes/{processInstanceId}/variables") 
                   ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting process variables: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public async Task<bool> SetProcessVariablesAsync(string processInstanceId, Dictionary<string, object> variables)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/camunda/processes/{processInstanceId}/variables", variables);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting process variables: {ex.Message}");
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetTaskVariablesAsync(string taskId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Dictionary<string, object>>($"api/camunda/tasks/{taskId}/variables") 
                   ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting task variables: {ex.Message}");
            return new Dictionary<string, object>();
        }
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DashboardStatsDto>("api/camunda/dashboard");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting dashboard stats: {ex.Message}");
            return null;
        }
    }

    // ========== Health Check ==========

    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/camunda/health");
            if (response.IsSuccessStatusCode)
            {
                return new HealthCheckResult { IsHealthy = true, Message = "Engine Online" };
            }
            
            var error = await response.Content.ReadFromJsonAsync<HealthErrorResponse>();
            return new HealthCheckResult 
            { 
                IsHealthy = false, 
                Message = error?.Message ?? $"Engine Unreachable ({response.StatusCode})" 
            };
        }
        catch (Exception ex)
        {
            return new HealthCheckResult { IsHealthy = false, Message = $"Connection Error: {ex.Message}" };
        }
    }

    private class HealthErrorResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

public class HealthCheckResult
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
}
