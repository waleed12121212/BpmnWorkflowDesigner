using BpmnWorkflow.Application.DTOs.Camunda;
using BpmnWorkflow.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Zeebe.Client;
using Zeebe.Client.Api.Responses;

namespace BpmnWorkflow.Application.Services;

/// <summary>
/// Implementation of Camunda Platform 8 Operate REST API integration
/// </summary>
public class Camunda8Service : ICamundaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICamundaEnvironmentService _envService;
    private readonly ILogger<Camunda8Service> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public Camunda8Service(IHttpClientFactory httpClientFactory, ICamundaEnvironmentService envService, ILogger<Camunda8Service> logger)
    {
        _httpClientFactory = httpClientFactory;
        _envService = envService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private async Task<HttpClient> GetOperateClientAsync(Guid? environmentId = null)
    {
        var env = await GetEnvironmentAsync(environmentId);

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(env.BaseUrl.TrimEnd('/') + "/v1/");
        
        // Local self-managed Operate usually uses demo/demo
        if (!string.IsNullOrEmpty(env.Username) && !string.IsNullOrEmpty(env.Password))
        {
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{env.Username}:{env.Password}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
        }

        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    // ========== Deployment Operations ==========

    public async Task<DeployWorkflowResponse> DeployWorkflowAsync(DeployWorkflowRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try 
        {
            var env = await GetEnvironmentAsync(environmentId);
            
            // Try Zeebe gRPC first (most reliable for C8)
            using var zeebeClient = GetZeebeClient(env);
            
            // Ensure at least one process is executable (Required for C8)
            var xml = request.BpmnXml;
            try
            {
                var xdoc = XDocument.Parse(xml);
                var nsBpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";
                var processes = xdoc.Descendants(XName.Get("process", nsBpmn)).ToList();
                
                bool modified = false;
                foreach (var process in processes)
                {
                    var attr = process.Attribute("isExecutable");
                    if (attr == null || attr.Value.ToLower() != "true")
                    {
                        process.SetAttributeValue("isExecutable", "true");
                        modified = true;
                    }
                }

                if (modified)
                {
                    xml = xdoc.ToString();
                    _logger.LogInformation("Modified BPMN XML to include isExecutable='true' for C8 deployment");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse/modify BPMN XML for isExecutable check. Sending original XML.");
            }

            var deploymentResponse = await zeebeClient.NewDeployCommand()
                .AddResourceString(xml, Encoding.UTF8, request.DeploymentName + ".bpmn")
                .Send(cancellationToken);

            var deployedProcess = deploymentResponse.Processes.FirstOrDefault();
            
            return new DeployWorkflowResponse
            {
                DeploymentId = deploymentResponse.Key.ToString(),
                ProcessDefinitionId = deployedProcess?.ProcessDefinitionKey.ToString() ?? "0",
                ProcessDefinitionKey = deployedProcess?.BpmnProcessId ?? "",
                DeploymentName = request.DeploymentName,
                DeploymentTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Zeebe gRPC Deployment failure for environment {EnvId}", environmentId);
             throw new Exception($"Failed to deploy to C8 via Zeebe gRPC: {ex.Message}", ex);
        }
    }

    private IZeebeClient GetZeebeClient(CamundaEnvironmentDto env)
    {
        // Extract host and port from ZeebeGatewayUrl
        var gateway = env.ZeebeGatewayUrl ?? "localhost:26500";
        
        // Remove protocol if present (Zeebe client adds it)
        if (gateway.StartsWith("http://")) gateway = gateway.Substring(7);
        if (gateway.StartsWith("https://")) gateway = gateway.Substring(8);
        
        return ZeebeClient.Builder()
            .UseGatewayAddress(gateway)
            .UsePlainText()
            .Build();
    }

    private async Task<CamundaEnvironmentDto> GetEnvironmentAsync(Guid? environmentId)
    {
        CamundaEnvironmentDto? env = null;
        if (environmentId.HasValue)
            env = await _envService.GetByIdAsync(environmentId.Value);
        else
            env = await _envService.GetActiveAsync();

        if (env == null) throw new InvalidOperationException("No active Camunda environment found.");
        return env;
    }

    public Task<DeployWorkflowResponse> DeployDmnAsync(Guid dmnId, string name, string xml, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("DMN deployment for Camunda 8 requires Zeebe gRPC client.");
    }

    public async Task<List<ProcessDefinitionDto>> GetProcessDefinitionsAsync(Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetOperateClientAsync(environmentId);
            // Operate uses search with POST
            var searchRequest = new { filter = new { }, size = 100 };
            var response = await client.PostAsJsonAsync("process-definitions/search", searchRequest, _jsonOptions, cancellationToken);
            
            if (!response.IsSuccessStatusCode) return new List<ProcessDefinitionDto>();

            var result = await response.Content.ReadFromJsonAsync<OperateSearchResponse<OperateProcessDefinition>>(_jsonOptions, cancellationToken);
            
            return result?.Items.Select(d => new ProcessDefinitionDto
            {
                Id = d.Key.ToString(),
                Key = d.BpmnProcessId,
                Name = d.Name ?? d.BpmnProcessId,
                Version = d.Version,
                Category = "Camunda 8",
                Suspended = false // Operate doesn't expose suspension state directly like C7
            }).ToList() ?? new List<ProcessDefinitionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting C8 process definitions");
            return new List<ProcessDefinitionDto>();
        }
    }

    public async Task<ProcessDefinitionDto?> GetProcessDefinitionByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var all = await GetProcessDefinitionsAsync(null, cancellationToken);
        return all.OrderByDescending(d => d.Version).FirstOrDefault(d => d.Key == key);
    }

    public Task<bool> DeleteDeploymentAsync(string deploymentId, bool cascade = true, CancellationToken cancellationToken = default)
    {
        // Deleting deployments is not a standard REST action in C8 Operate
        throw new NotImplementedException("Deployment deletion is not supported via Operate REST API.");
    }

    public async Task<string?> GetProcessDefinitionXmlAsync(string processDefinitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetOperateClientAsync(); // Usually same env

            var response = await client.GetAsync($"process-definitions/{processDefinitionId}/xml", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting C8 BPMN XML");
            return null;
        }
    }

    // ========== Dashboard Stats ==========

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = new DashboardStatsDto();
            var client = await GetOperateClientAsync(environmentId);

            // 1. Active Instances
            var activeRes = await client.PostAsJsonAsync("process-instances/search", new { filter = new { state = "ACTIVE" }, size = 1 }, _jsonOptions, cancellationToken);
            if (activeRes.IsSuccessStatusCode) {
                var res = await activeRes.Content.ReadFromJsonAsync<OperateSearchResponse<object>>(_jsonOptions, cancellationToken);
                stats.ActiveInstances = res?.Total ?? 0;
            }

            // 2. Completed Instances
            var completedRes = await client.PostAsJsonAsync("process-instances/search", new { filter = new { state = "COMPLETED" }, size = 1 }, _jsonOptions, cancellationToken);
            if (completedRes.IsSuccessStatusCode) {
                var res = await completedRes.Content.ReadFromJsonAsync<OperateSearchResponse<object>>(_jsonOptions, cancellationToken);
                stats.CompletedInstances = res?.Total ?? 0;
            }

            // 3. Incidents
            var incidentRes = await client.PostAsJsonAsync("incidents/search", new { filter = new { state = "ACTIVE" }, size = 1 }, _jsonOptions, cancellationToken);
            if (incidentRes.IsSuccessStatusCode) {
                var res = await incidentRes.Content.ReadFromJsonAsync<OperateSearchResponse<object>>(_jsonOptions, cancellationToken);
                stats.IncidentCount = res?.Total ?? 0;
            }

            stats.TotalInstances = stats.ActiveInstances + stats.CompletedInstances;

            // 4. Process-wise stats
            var definitions = await GetProcessDefinitionsAsync(environmentId, cancellationToken);
            foreach (var def in definitions.GroupBy(d => d.Key).Select(g => g.OrderByDescending(x => x.Version).First()))
            {
                var pRes = await client.PostAsJsonAsync("process-instances/search", new { filter = new { bpmnProcessId = def.Key, state = "ACTIVE" }, size = 1 }, _jsonOptions, cancellationToken);
                int active = 0;
                if (pRes.IsSuccessStatusCode) {
                    active = (await pRes.Content.ReadFromJsonAsync<OperateSearchResponse<object>>(_jsonOptions, cancellationToken))?.Total ?? 0;
                }

                stats.ProcessStats.Add(new ProcessStatsDto {
                    ProcessDefinitionKey = def.Key,
                    ProcessName = def.Name,
                    ActiveInstances = active
                });
            }

            // 5. Daily trends (simplified for C8)
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                stats.DailyStats.Add(new DailyInstanceStatsDto {
                    Date = date,
                    StartedCount = 0, // Operate doesn't easily give count per day in a single simple call without history cleanup
                    FinishedCount = 0
                });
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating C8 dashboard stats");
            return new DashboardStatsDto { 
                ActiveInstances = 0, 
                ProcessStats = new List<ProcessStatsDto>() 
            };
        }
    }

    public async Task<bool> IsHealthyAsync(Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetOperateClientAsync(environmentId);
            // Camunda 8 Operate REST API is POST-based for most resources
            // IMPORTANT: size must be >= 1 according to API rules
            var searchRequest = new { filter = new { }, size = 1 };
            var response = await client.PostAsJsonAsync("process-definitions/search", searchRequest, _jsonOptions, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Camunda 8 Health Check failed with {StatusCode}: {Content}. URL: {Url}", 
                    response.StatusCode, content, client.BaseAddress + "process-definitions/search");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Camunda 8 Health Check connect to Operate");
            return false;
        }
    }

    // Interfaces stubs for dashboard
    public Task<List<ActivityInstanceDto>> GetHistoryActivitiesAsync(string processInstanceId, CancellationToken cancellationToken = default) => Task.FromResult(new List<ActivityInstanceDto>());
    public Task<List<ActivityStatsDto>> GetProcessDefinitionStatisticsAsync(string processDefinitionId, CancellationToken cancellationToken = default) => Task.FromResult(new List<ActivityStatsDto>());
    public async Task<ProcessInstanceDto> StartProcessInstanceAsync(StartProcessInstanceRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var env = await GetEnvironmentAsync(environmentId);
            using var zeebeClient = GetZeebeClient(env);
            
            var startCommand = zeebeClient.NewCreateProcessInstanceCommand()
                .BpmnProcessId(request.ProcessDefinitionKey)
                .LatestVersion();

            if (request.Variables != null && request.Variables.Any())
            {
                var variablesJson = JsonSerializer.Serialize(request.Variables, _jsonOptions);
                startCommand.Variables(variablesJson);
            }

            var result = await startCommand.Send(cancellationToken);

            return new ProcessInstanceDto
            {
                Id = result.ProcessInstanceKey.ToString(),
                ProcessDefinitionId = result.ProcessDefinitionKey.ToString(),
                ProcessDefinitionKey = request.ProcessDefinitionKey,
                StartTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting C8 process instance via gRPC");
            throw new Exception($"Failed to start C8 process via Zeebe gRPC: {ex.Message}", ex);
        }
    }
    public async Task<List<ProcessInstanceDto>> GetProcessInstancesAsync(string? processDefinitionKey = null, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetOperateClientAsync(environmentId); // Uses active env for list
            var filter = processDefinitionKey != null ? new { bpmnProcessId = processDefinitionKey } : new object { };
            var searchRequest = new 
            { 
                filter = filter, 
                size = 100,
                sort = new[] { new { field = "startDate", order = "DESC" } }
            };
            
            var response = await client.PostAsJsonAsync("process-instances/search", searchRequest, _jsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode) return new List<ProcessInstanceDto>();

            var result = await response.Content.ReadFromJsonAsync<OperateSearchResponse<OperateProcessInstance>>(_jsonOptions, cancellationToken);
            
            return result?.Items.Select(i => new ProcessInstanceDto
            {
                Id = i.Key.ToString(),
                ProcessDefinitionId = i.ProcessDefinitionKey.ToString(),
                ProcessDefinitionKey = i.BpmnProcessId,
                ProcessDefinitionName = i.ProcessDefinitionName ?? i.BpmnProcessId,
                BusinessKey = null, // C8 uses variables or specific business keys if indexed
                Ended = i.State == "COMPLETED" || i.State == "CANCELED",
                Suspended = false,
                StartTime = i.StartDate,
                EndTime = i.EndDate
            }).ToList() ?? new List<ProcessInstanceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting C8 process instances");
            return new List<ProcessInstanceDto>();
        }
    }

    public async Task<ProcessInstanceDto?> GetProcessInstanceAsync(string processInstanceId, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetOperateClientAsync(environmentId);
            var response = await client.GetAsync($"process-instances/{processInstanceId}", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var i = await response.Content.ReadFromJsonAsync<OperateProcessInstance>(_jsonOptions, cancellationToken);
            if (i == null) return null;

            return new ProcessInstanceDto
            {
                Id = i.Key.ToString(),
                ProcessDefinitionId = i.ProcessDefinitionKey.ToString(),
                ProcessDefinitionKey = i.BpmnProcessId,
                ProcessDefinitionName = i.ProcessDefinitionName ?? i.BpmnProcessId,
                Ended = i.State == "COMPLETED" || i.State == "CANCELED",
                Suspended = false,
                StartTime = i.StartDate,
                EndTime = i.EndDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting C8 process instance {Id}", processInstanceId);
            return null;
        }
    }
    public Task<bool> DeleteProcessInstanceAsync(string processInstanceId, string? reason = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<ActivityInstanceDto?> GetActivityInstanceAsync(string processInstanceId, CancellationToken cancellationToken = default) => Task.FromResult<ActivityInstanceDto?>(null);
    public Task<List<UserTaskDto>> GetUserTasksAsync(string? assignee = null, string? processInstanceId = null, Guid? environmentId = null, CancellationToken cancellationToken = default) => Task.FromResult(new List<UserTaskDto>());
    public Task<UserTaskDto?> GetUserTaskAsync(string taskId, CancellationToken cancellationToken = default) => Task.FromResult<UserTaskDto?>(null);
    public Task<bool> ClaimTaskAsync(string taskId, string userId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<bool> UnclaimTaskAsync(string taskId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<bool> CompleteTaskAsync(string taskId, CompleteTaskRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<Dictionary<string, object>> GetProcessVariablesAsync(string processInstanceId, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, object>());
    public Task<bool> SetProcessVariablesAsync(string processInstanceId, Dictionary<string, object> variables, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<Dictionary<string, object>> GetTaskVariablesAsync(string taskId, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, object>());
    public Task<List<ExternalTaskDto>> FetchAndLockExternalTasksAsync(FetchExternalTasksRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new List<ExternalTaskDto>());
    public Task<bool> CompleteExternalTaskAsync(string taskId, CompleteExternalTaskRequest request, CancellationToken cancellationToken = default) => Task.FromResult(false);
    public Task<bool> ReportExternalTaskFailureAsync(string taskId, string workerId, string errorMessage, int retries = 3, CancellationToken cancellationToken = default) => Task.FromResult(false);

    // Internal Models for Operate
    private class OperateSearchResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
    }

    private class OperateProcessDefinition
    {
        public long Key { get; set; }
        public string BpmnProcessId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public int Version { get; set; }
    }

    private class OperateProcessInstance
    {
        public long Key { get; set; }
        public long ProcessDefinitionKey { get; set; }
        public string BpmnProcessId { get; set; } = string.Empty;
        public string? ProcessDefinitionName { get; set; }
        public string State { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    private class C8DeploymentResponse
    {
        public long DeploymentKey { get; set; }
        public List<C8DeployedProcess>? Processes { get; set; }
    }

    private class C8DeployedProcess
    {
        public long ProcessDefinitionKey { get; set; }
        public string BpmnProcessId { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    private class C8StartInstanceResponse
    {
        public long ProcessInstanceKey { get; set; }
        public long ProcessDefinitionKey { get; set; }
    }
}
