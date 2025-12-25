using BpmnWorkflow.Application.DTOs.Camunda;
using BpmnWorkflow.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace BpmnWorkflow.Application.Services;

/// <summary>
/// Implementation of Camunda Platform 7 REST API integration
/// </summary>
public class CamundaService : ICamundaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ICamundaEnvironmentService _envService;
    private readonly ILogger<CamundaService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CamundaService(IHttpClientFactory httpClientFactory, ICamundaEnvironmentService envService, ILogger<CamundaService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _envService = envService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters = { 
                new System.Text.Json.Serialization.JsonStringEnumConverter(),
                new CamundaDateTimeConverter()
            }
        };
    }

    private async Task<HttpClient> GetClientAsync(Guid? environmentId = null)
    {
        CamundaEnvironmentDto? env = null;
        if (environmentId.HasValue)
        {
            env = await _envService.GetByIdAsync(environmentId.Value);
        }
        else
        {
            env = await _envService.GetActiveAsync();
        }

        if (env == null)
        {
            throw new InvalidOperationException("No active Camunda environment found. Please configure and activate an environment in the Camunda Environments page.");
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(env.BaseUrl);
        
        if (!string.IsNullOrEmpty(env.Username) && !string.IsNullOrEmpty(env.Password))
        {
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{env.Username}:{env.Password}"));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
        }

        return client;
    }

    // ========== Deployment Operations ==========

    public async Task<DeployWorkflowResponse> DeployWorkflowAsync(DeployWorkflowRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deploying workflow {WorkflowId} to Camunda. Name: {Name}, XML Size: {Size} bytes", 
                request.WorkflowId, request.DeploymentName, request.BpmnXml?.Length ?? 0);

            // Ensure History TTL is present (required by newer Camunda 7 versions)
            var xml = request.BpmnXml;
            if (!xml.Contains("camunda:historyTimeToLive"))
            {
                _logger.LogInformation("Injecting default historyTimeToLive into BPMN XML");
                try
                {
                    var xdoc = System.Xml.Linq.XDocument.Parse(xml);
                    var nsCamunda = "http://camunda.org/schema/1.0/bpmn";
                    var nsBpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";
                    
                    // Add camunda namespace if not present
                    if (xdoc.Root != null)
                    {
                        var camundaPrefix = xdoc.Root.GetPrefixOfNamespace(nsCamunda);
                        if (string.IsNullOrEmpty(camundaPrefix))
                        {
                            xdoc.Root.Add(new System.Xml.Linq.XAttribute(System.Xml.Linq.XNamespace.Xmlns + "camunda", nsCamunda));
                        }
                        
                        var processes = xdoc.Descendants(System.Xml.Linq.XName.Get("process", nsBpmn));
                        foreach (var process in processes)
                        {
                            if (process.Attribute(System.Xml.Linq.XName.Get("historyTimeToLive", nsCamunda)) == null)
                            {
                                process.Add(new System.Xml.Linq.XAttribute(System.Xml.Linq.XName.Get("historyTimeToLive", nsCamunda), "180"));
                            }
                            
                            // Also ensure isExecutable is true
                            var isExecAttr = process.Attribute("isExecutable");
                            if (isExecAttr == null || isExecAttr.Value.ToLower() != "true")
                            {
                                process.SetAttributeValue("isExecutable", "true");
                            }
                        }
                        xml = xdoc.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to inject historyTimeToLive, sending original XML");
                }
            }

            using var content = new MultipartFormDataContent();
            
            // Add deployment name
            content.Add(new StringContent(request.DeploymentName), "deployment-name");
            
            // Add enable duplicate filtering
            content.Add(new StringContent(request.EnableDuplicateFiltering.ToString().ToLower()), "enable-duplicate-filtering");
            
            // Add deploy changed only
            content.Add(new StringContent(request.DeployChangedOnly.ToString().ToLower()), "deploy-changed-only");
            
            // Add BPMN file
            var bpmnContent = new StringContent(xml ?? request.BpmnXml, Encoding.UTF8, "application/xml");
            content.Add(bpmnContent, "data", "process.bpmn");

            var client = await GetClientAsync(environmentId);
            _logger.LogInformation("Sending POST to {Url}", client.BaseAddress + "deployment/create");
            
            var response = await client.PostAsync("deployment/create", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Camunda deployment failed with status {StatusCode}: {Body}", response.StatusCode, errorBody);
                
                throw new Exception($"Camunda Deployment Error ({response.StatusCode}): {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<CamundaDeploymentResponse>(_jsonOptions, cancellationToken);
            
            if (result == null)
                throw new Exception("Failed to deserialize deployment response");
            
            if (result.DeployedProcessDefinitions == null || !result.DeployedProcessDefinitions.Any())
            {
                throw new InvalidOperationException("Deployment succeeded but no executable process definitions were found. Please ensure 'Executable' (isExecutable) is set to true in the Process Settings of your diagram.");
            }

            var processDefinition = result.DeployedProcessDefinitions.Values.First();
            
            return new DeployWorkflowResponse
            {
                DeploymentId = result.Id,
                ProcessDefinitionId = processDefinition.Id,
                ProcessDefinitionKey = processDefinition.Key,
                DeploymentTime = result.DeploymentTime,
                DeploymentName = result.Name
            };
        }
        catch (InvalidOperationException) { throw; } // Re-throw to be caught by controller
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Connection error to Camunda while deploying workflow {WorkflowId}", request.WorkflowId);
            throw new Exception("Cannot connect to Camunda engine or the request failed. Please ensure the Camunda service is running.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout connecting to Camunda while deploying workflow {WorkflowId}", request.WorkflowId);
            throw new Exception("Connection to Camunda engine timed out.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying workflow {WorkflowId}", request.WorkflowId);
            throw;
        }
    }

    public async Task<DeployWorkflowResponse> DeployDmnAsync(Guid dmnId, string name, string xml, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deploying DMN {DmnId} to Camunda", dmnId);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(name), "deployment-name");
            content.Add(new StringContent("true"), "enable-duplicate-filtering");
            content.Add(new StringContent("true"), "deploy-changed-only");
            
            var dmnContent = new StringContent(xml, Encoding.UTF8, "application/xml");
            content.Add(dmnContent, "data", $"{name}.dmn");

            var client = await GetClientAsync();
            var response = await client.PostAsync("deployment/create", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CamundaDeploymentResponse>(_jsonOptions, cancellationToken);
            
            if (result == null)
                throw new Exception("Failed to deserialize deployment response");

            return new DeployWorkflowResponse
            {
                DeploymentId = result.Id,
                DeploymentTime = result.DeploymentTime,
                DeploymentName = result.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deploying DMN {DmnId}", dmnId);
            throw;
        }
    }

    public async Task<List<ProcessDefinitionDto>> GetProcessDefinitionsAsync(Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync(environmentId);
            var response = await client.GetAsync("process-definition?latestVersion=true", cancellationToken);
            response.EnsureSuccessStatusCode();

            var definitions = await response.Content.ReadFromJsonAsync<List<CamundaProcessDefinition>>(_jsonOptions, cancellationToken);
            
            return definitions?.Select(d => new ProcessDefinitionDto
            {
                Id = d.Id,
                Key = d.Key,
                Name = d.Name ?? d.Key,
                Version = d.Version,
                Category = d.Category,
                DeploymentId = d.DeploymentId,
                Suspended = d.Suspended,
                VersionTag = d.VersionTag
            }).ToList() ?? new List<ProcessDefinitionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definitions");
            throw;
        }
    }

    public async Task<ProcessDefinitionDto?> GetProcessDefinitionByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"process-definition/key/{key}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var definition = await response.Content.ReadFromJsonAsync<CamundaProcessDefinition>(_jsonOptions, cancellationToken);
            
            if (definition == null)
                return null;

            return new ProcessDefinitionDto
            {
                Id = definition.Id,
                Key = definition.Key,
                Name = definition.Name ?? definition.Key,
                Version = definition.Version,
                Category = definition.Category,
                DeploymentId = definition.DeploymentId,
                Suspended = definition.Suspended,
                VersionTag = definition.VersionTag
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definition by key {Key}", key);
            throw;
        }
    }

    public async Task<bool> DeleteDeploymentAsync(string deploymentId, bool cascade = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting deployment {DeploymentId}", deploymentId);

            var client = await GetClientAsync();
            var response = await client.DeleteAsync($"deployment/{deploymentId}?cascade={cascade.ToString().ToLower()}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting deployment {DeploymentId}", deploymentId);
            throw;
        }
    }

    public async Task<string?> GetProcessDefinitionXmlAsync(string processDefinitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"process-definition/{processDefinitionId}/xml", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<CamundaXmlResponse>(_jsonOptions, cancellationToken);
            return result?.Bpmn20Xml;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definition XML for {Id}", processDefinitionId);
            return null;
        }
    }

    // ========== Process Instance Operations ==========

    public async Task<ProcessInstanceDto> StartProcessInstanceAsync(StartProcessInstanceRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting C7 process instance for key {Key}", request.ProcessDefinitionKey);

            var payload = new
            {
                businessKey = request.BusinessKey,
                variables = request.Variables?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new { value = kvp.Value, type = GetCamundaType(kvp.Value) }
                )
            };

            var client = await GetClientAsync(environmentId);
            var response = await client.PostAsJsonAsync($"process-definition/key/{request.ProcessDefinitionKey}/start", payload, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            var instance = await response.Content.ReadFromJsonAsync<CamundaProcessInstance>(_jsonOptions, cancellationToken);
            
            if (instance == null)
                throw new Exception("Failed to deserialize process instance response");

            return new ProcessInstanceDto
            {
                Id = instance.Id,
                ProcessDefinitionId = instance.DefinitionId,
                BusinessKey = instance.BusinessKey,
                Ended = instance.Ended,
                Suspended = instance.Suspended
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting process instance");
            throw;
        }
    }

    public async Task<ProcessInstanceDto?> GetProcessInstanceAsync(string processInstanceId, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync(environmentId);
            var response = await client.GetAsync($"process-instance/{processInstanceId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var instance = await response.Content.ReadFromJsonAsync<CamundaProcessInstance>(_jsonOptions, cancellationToken);
            
            if (instance == null)
                return null;

            return new ProcessInstanceDto
            {
                Id = instance.Id,
                ProcessDefinitionId = instance.DefinitionId,
                BusinessKey = instance.BusinessKey,
                Ended = instance.Ended,
                Suspended = instance.Suspended
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instance {ProcessInstanceId}", processInstanceId);
            throw;
        }
    }

    public async Task<List<ProcessInstanceDto>> GetProcessInstancesAsync(string? processDefinitionKey = null, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = "process-instance";
            if (!string.IsNullOrEmpty(processDefinitionKey))
                url += $"?processDefinitionKey={processDefinitionKey}";

            var client = await GetClientAsync(environmentId);
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var instances = await response.Content.ReadFromJsonAsync<List<CamundaProcessInstance>>(_jsonOptions, cancellationToken);
            
            if (instances == null || !instances.Any())
                return new List<ProcessInstanceDto>();

            // Fetch definitions to map names
            var definitions = await GetProcessDefinitionsAsync(null, cancellationToken);
            var definitionMap = definitions.ToDictionary(d => d.Id, d => d);

            return instances.Select(i => 
            {
                var definition = definitionMap.TryGetValue(i.DefinitionId, out var d) ? d : null;
                return new ProcessInstanceDto
                {
                    Id = i.Id,
                    ProcessDefinitionId = i.DefinitionId,
                    BusinessKey = i.BusinessKey,
                    Ended = i.Ended,
                    Suspended = i.Suspended,
                    ProcessDefinitionKey = definition?.Key ?? i.DefinitionId,
                    ProcessDefinitionName = definition?.Name ?? definition?.Key ?? i.DefinitionId
                };
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instances");
            throw;
        }
    }

    public async Task<List<ActivityStatsDto>> GetProcessDefinitionStatisticsAsync(string processDefinitionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"process-definition/{processDefinitionId}/statistics?incidents=true", cancellationToken);
            response.EnsureSuccessStatusCode();

            var rawStats = await response.Content.ReadFromJsonAsync<List<CamundaProcessDefinitionStats>>(_jsonOptions, cancellationToken);
            
            return rawStats?.Select(rs => new ActivityStatsDto
            {
                ActivityId = rs.Id,
                Instances = rs.Instances,
                Failed = rs.Failed,
                IncidentCount = rs.Incidents?.Sum(i => i.IncidentCount) ?? 0
            }).ToList() ?? new List<ActivityStatsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definition statistics for {Id}", processDefinitionId);
            throw;
        }
    }

    public async Task<List<ActivityInstanceDto>> GetHistoryActivitiesAsync(string processInstanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"history/activity-instance?processInstanceId={processInstanceId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var historyItems = await response.Content.ReadFromJsonAsync<List<CamundaHistoryActivity>>(_jsonOptions, cancellationToken);
            
            return historyItems?.Select(h => new ActivityInstanceDto
            {
                Id = h.Id,
                ActivityId = h.ActivityId,
                ActivityName = h.ActivityName ?? h.ActivityId,
                ActivityType = h.ActivityType,
                ProcessInstanceId = h.ProcessInstanceId
            }).ToList() ?? new List<ActivityInstanceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history activities for {Id}", processInstanceId);
            throw;
        }
    }

    public async Task<bool> DeleteProcessInstanceAsync(string processInstanceId, string? reason = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting process instance {ProcessInstanceId}", processInstanceId);

            var url = $"process-instance/{processInstanceId}";
            if (!string.IsNullOrEmpty(reason))
                url += $"?skipCustomListeners=false&skipIoMappings=false&failIfNotExists=true";

            var client = await GetClientAsync();
            var response = await client.DeleteAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting process instance {ProcessInstanceId}", processInstanceId);
            throw;
        }
    }

    public async Task<ActivityInstanceDto?> GetActivityInstanceAsync(string processInstanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"process-instance/{processInstanceId}/activity-instances", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var activity = await response.Content.ReadFromJsonAsync<CamundaActivityInstance>(_jsonOptions, cancellationToken);
            
            return activity != null ? MapActivityInstance(activity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activity instance for {ProcessInstanceId}", processInstanceId);
            throw;
        }
    }

    // ========== User Task Operations ==========

    public async Task<List<UserTaskDto>> GetUserTasksAsync(string? assignee = null, string? processInstanceId = null, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(assignee))
                queryParams.Add($"assignee={assignee}");
            if (!string.IsNullOrEmpty(processInstanceId))
                queryParams.Add($"processInstanceId={processInstanceId}");

            var url = "task";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var client = await GetClientAsync(environmentId);
            var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tasks = await response.Content.ReadFromJsonAsync<List<CamundaTask>>(_jsonOptions, cancellationToken);
            
            return tasks?.Select(t => new UserTaskDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                ProcessInstanceId = t.ProcessInstanceId,
                ProcessDefinitionId = t.ProcessDefinitionId,
                Assignee = t.Assignee,
                Created = t.Created,
                Due = t.Due,
                FollowUp = t.FollowUp,
                Priority = t.Priority,
                TaskDefinitionKey = t.TaskDefinitionKey,
                FormKey = t.FormKey
            }).ToList() ?? new List<UserTaskDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user tasks");
            throw;
        }
    }

    public async Task<UserTaskDto?> GetUserTaskAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"task/{taskId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var task = await response.Content.ReadFromJsonAsync<CamundaTask>(_jsonOptions, cancellationToken);
            
            if (task == null)
                return null;

            return new UserTaskDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                ProcessInstanceId = task.ProcessInstanceId,
                ProcessDefinitionId = task.ProcessDefinitionId,
                Assignee = task.Assignee,
                Created = task.Created,
                Due = task.Due,
                FollowUp = task.FollowUp,
                Priority = task.Priority,
                TaskDefinitionKey = task.TaskDefinitionKey,
                FormKey = task.FormKey
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user task {TaskId}", taskId);
            throw;
        }
    }

    public async Task<bool> ClaimTaskAsync(string taskId, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Claiming task {TaskId} for user {UserId}", taskId, userId);

            var client = await GetClientAsync();
            var payload = new { userId };
            var response = await client.PostAsJsonAsync($"task/{taskId}/claim", payload, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming task {TaskId}", taskId);
            throw;
        }
    }

    public async Task<bool> UnclaimTaskAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unclaiming task {TaskId}", taskId);

            var client = await GetClientAsync();
            var response = await client.PostAsync($"task/{taskId}/unclaim", null, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unclaiming task {TaskId}", taskId);
            throw;
        }
    }

    public async Task<bool> CompleteTaskAsync(string taskId, CompleteTaskRequest request, Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Completing C7 task {TaskId}", taskId);

            var payload = new
            {
                variables = request.Variables?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new { value = kvp.Value, type = GetCamundaType(kvp.Value) }
                )
            };

            var client = await GetClientAsync(environmentId);
            var response = await client.PostAsJsonAsync($"task/{taskId}/complete", payload, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {TaskId}", taskId);
            throw;
        }
    }

    // ========== Process Variables Operations ==========

    public async Task<Dictionary<string, object>> GetProcessVariablesAsync(string processInstanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"process-instance/{processInstanceId}/variables", cancellationToken);
            response.EnsureSuccessStatusCode();

            var variables = await response.Content.ReadFromJsonAsync<Dictionary<string, CamundaVariable>>(_jsonOptions, cancellationToken);
            
            return variables?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value) ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process variables for {ProcessInstanceId}", processInstanceId);
            throw;
        }
    }

    public async Task<bool> SetProcessVariablesAsync(string processInstanceId, Dictionary<string, object> variables, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                modifications = variables.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new { value = kvp.Value, type = GetCamundaType(kvp.Value) }
                )
            };

            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync($"process-instance/{processInstanceId}/variables", payload, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting process variables for {ProcessInstanceId}", processInstanceId);
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetTaskVariablesAsync(string taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"task/{taskId}/variables", cancellationToken);
            response.EnsureSuccessStatusCode();

            var variables = await response.Content.ReadFromJsonAsync<Dictionary<string, CamundaVariable>>(_jsonOptions, cancellationToken);
            
            return variables?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value) ?? new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task variables for {TaskId}", taskId);
            throw;
        }
    }

    // ========== External Task Operations ==========

    public async Task<List<ExternalTaskDto>> FetchAndLockExternalTasksAsync(FetchExternalTasksRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                workerId = request.WorkerId,
                maxTasks = request.MaxTasks,
                usePriority = request.UsePriority,
                topics = request.Topics.Select(t => new
                {
                    topicName = t.TopicName,
                    lockDuration = t.LockDuration,
                    variables = t.Variables
                })
            };

            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync("external-task/fetchAndLock", payload, _jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            var tasks = await response.Content.ReadFromJsonAsync<List<CamundaExternalTask>>(_jsonOptions, cancellationToken);
            
            return tasks?.Select(t => new ExternalTaskDto
            {
                Id = t.Id,
                TopicName = t.TopicName,
                WorkerId = t.WorkerId,
                ProcessInstanceId = t.ProcessInstanceId,
                ProcessDefinitionId = t.ProcessDefinitionId,
                ActivityId = t.ActivityId,
                LockExpirationTime = t.LockExpirationTime,
                Retries = t.Retries,
                Variables = t.Variables?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value)
            }).ToList() ?? new List<ExternalTaskDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching external tasks");
            throw;
        }
    }

    public async Task<bool> CompleteExternalTaskAsync(string taskId, CompleteExternalTaskRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                workerId = request.WorkerId,
                variables = request.Variables?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new { value = kvp.Value, type = GetCamundaType(kvp.Value) }
                )
            };

            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync($"external-task/{taskId}/complete", payload, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing external task {TaskId}", taskId);
            throw;
        }
    }

    public async Task<bool> ReportExternalTaskFailureAsync(string taskId, string workerId, string errorMessage, int retries = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                workerId,
                errorMessage,
                retries,
                retryTimeout = 60000 // 1 minute
            };

            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync($"external-task/{taskId}/failure", payload, _jsonOptions, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting external task failure {TaskId}", taskId);
            throw;
        }
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = new DashboardStatsDto();
            var client = await GetClientAsync(environmentId);

            // 1. Get Active Process Instances Count
            var activeResponse = await client.GetAsync("process-instance/count", cancellationToken);
            if (activeResponse.IsSuccessStatusCode)
            {
                var countObj = await activeResponse.Content.ReadFromJsonAsync<CamundaCountResponse>(_jsonOptions, cancellationToken);
                stats.ActiveInstances = countObj?.Count ?? 0;
            }

            // 2. Get Completed Process Instances Count
            var completedResponse = await client.GetAsync("history/process-instance/count?finished=true", cancellationToken);
            if (completedResponse.IsSuccessStatusCode)
            {
                var countObj = await completedResponse.Content.ReadFromJsonAsync<CamundaCountResponse>(_jsonOptions, cancellationToken);
                stats.CompletedInstances = countObj?.Count ?? 0;
            }
            stats.TotalInstances = stats.ActiveInstances + stats.CompletedInstances;

            // 3. Get Active Tasks Count
            var tasksResponse = await client.GetAsync("task/count", cancellationToken);
            if (tasksResponse.IsSuccessStatusCode)
            {
                var countObj = await tasksResponse.Content.ReadFromJsonAsync<CamundaCountResponse>(_jsonOptions, cancellationToken);
                stats.ActiveTasks = countObj?.Count ?? 0;
            }

            // 4. Get Overdue Tasks Count
            var nowStr = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var overdueResponse = await client.GetAsync($"task/count?dueBefore={nowStr}", cancellationToken);
            if (overdueResponse.IsSuccessStatusCode)
            {
                var countObj = await overdueResponse.Content.ReadFromJsonAsync<CamundaCountResponse>(_jsonOptions, cancellationToken);
                stats.OverdueTasks = countObj?.Count ?? 0;
            }

            // 5. Get Incident Count
            var incidentsResponse = await client.GetAsync("incident/count", cancellationToken);
            if (incidentsResponse.IsSuccessStatusCode)
            {
                var countObj = await incidentsResponse.Content.ReadFromJsonAsync<CamundaCountResponse>(_jsonOptions, cancellationToken);
                stats.IncidentCount = countObj?.Count ?? 0;
            }

            // 6. Get Per-Process Statistics
            var procStatsResponse = await client.GetAsync("process-definition/statistics?incidents=true", cancellationToken);
            if (procStatsResponse.IsSuccessStatusCode)
            {
                var rawStats = await procStatsResponse.Content.ReadFromJsonAsync<List<CamundaProcessDefinitionStats>>(_jsonOptions, cancellationToken);
                if (rawStats != null)
                {
                    // Get definition names for better display
                    var definitions = await GetProcessDefinitionsAsync(environmentId, cancellationToken);
                    
                    stats.ProcessStats = rawStats.Select(rs => new ProcessStatsDto
                    {
                        ProcessDefinitionKey = rs.Definition?.Key ?? "unknown",
                        ProcessName = definitions.FirstOrDefault(d => d.Id == rs.Definition?.Id)?.Name ?? rs.Definition?.Name ?? rs.Definition?.Key ?? "Unknown",
                        ActiveInstances = rs.Instances,
                        IncidentCount = rs.Incidents?.Sum(i => i.IncidentCount) ?? 0
                    }).ToList();
                }
            }

            // 7. Get Daily Trends (Last 7 Days)
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var startTime = date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                var endTime = date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                var startedReq = await client.GetAsync($"history/process-instance/count?startedAfter={startTime}&startedBefore={endTime}", cancellationToken);
                var finishedReq = await client.GetAsync($"history/process-instance/count?finishedAfter={startTime}&finishedBefore={endTime}", cancellationToken);

                var started = 0;
                var finished = 0;

                if (startedReq.IsSuccessStatusCode)
                {
                    started = (await startedReq.Content.ReadFromJsonAsync<CamundaCountResponse>(_jsonOptions, cancellationToken))?.Count ?? 0;
                }
                if (finishedReq.IsSuccessStatusCode)
                {
                    finished = (await finishedReq.Content.ReadFromJsonAsync<CamundaCountResponse>(_jsonOptions, cancellationToken))?.Count ?? 0;
                }

                stats.DailyStats.Add(new DailyInstanceStatsDto
                {
                    Date = date,
                    StartedCount = started,
                    FinishedCount = finished
                });
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard statistics from Camunda");
            throw;
        }
    }

    // ========== Health Check ==========

    public async Task<bool> IsHealthyAsync(Guid? environmentId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync(environmentId);
            var response = await client.GetAsync("version", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Camunda 7 Health Check Exception");
            return false;
        }
    }

    // ========== Helper Methods ==========

    private static string GetCamundaType(object value)
    {
        return value switch
        {
            int => "Integer",
            long => "Long",
            double or float => "Double",
            bool => "Boolean",
            DateTime => "Date",
            _ => "String"
        };
    }

    private static ActivityInstanceDto MapActivityInstance(CamundaActivityInstance activity)
    {
        return new ActivityInstanceDto
        {
            Id = activity.Id,
            ActivityId = activity.ActivityId,
            ActivityName = activity.ActivityName ?? activity.ActivityId,
            ActivityType = activity.ActivityType,
            ProcessInstanceId = activity.ProcessInstanceId,
            ChildActivityInstances = activity.ChildActivityInstances?.Select(MapActivityInstance).ToList()
        };
    }

    // ========== Internal Camunda Response Models ==========

    private class CamundaDeploymentResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime DeploymentTime { get; set; }
        public Dictionary<string, CamundaProcessDefinition>? DeployedProcessDefinitions { get; set; }
    }

    private class CamundaProcessDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string? Name { get; set; }
        public int Version { get; set; }
        public string? Category { get; set; }
        public string DeploymentId { get; set; } = string.Empty;
        public bool Suspended { get; set; }
        public string? VersionTag { get; set; }
    }

    private class CamundaProcessInstance
    {
        public string Id { get; set; } = string.Empty;
        public string DefinitionId { get; set; } = string.Empty;
        public string? BusinessKey { get; set; }
        public bool Ended { get; set; }
        public bool Suspended { get; set; }
    }

    private class CamundaTask
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

    private class CamundaVariable
    {
        public object Value { get; set; } = new();
        public string Type { get; set; } = "String";
    }

    private class CamundaExternalTask
    {
        public string Id { get; set; } = string.Empty;
        public string TopicName { get; set; } = string.Empty;
        public string WorkerId { get; set; } = string.Empty;
        public string ProcessInstanceId { get; set; } = string.Empty;
        public string ProcessDefinitionId { get; set; } = string.Empty;
        public string ActivityId { get; set; } = string.Empty;
        public DateTime? LockExpirationTime { get; set; }
        public int Retries { get; set; }
        public Dictionary<string, CamundaVariable>? Variables { get; set; }
    }

    private class CamundaActivityInstance
    {
        public string Id { get; set; } = string.Empty;
        public string ActivityId { get; set; } = string.Empty;
        public string? ActivityName { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string ProcessInstanceId { get; set; } = string.Empty;
        public List<CamundaActivityInstance>? ChildActivityInstances { get; set; }
    }

    private class CamundaDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String && DateTime.TryParse(reader.GetString(), out var date))
            {
                return date;
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

    private class CamundaXmlResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Bpmn20Xml { get; set; } = string.Empty;
    }

    private class CamundaHistoryActivity
    {
        public string Id { get; set; } = string.Empty;
        public string ActivityId { get; set; } = string.Empty;
        public string? ActivityName { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string ProcessInstanceId { get; set; } = string.Empty;
    }

    private class CamundaCountResponse
    {
        public int Count { get; set; }
    }

    private class CamundaProcessDefinitionStats
    {
        public string Id { get; set; } = string.Empty;
        public int Instances { get; set; }
        public int Failed { get; set; }
        public List<CamundaIncidentStats>? Incidents { get; set; }
        public CamundaProcessDefinition? Definition { get; set; }
    }

    private class CamundaIncidentStats
    {
        public string IncidentType { get; set; } = string.Empty;
        public int IncidentCount { get; set; }
    }
}
