using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Client.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly HttpClient _httpClient;

        public WorkflowService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(string? search = null)
        {
            var url = "api/workflows";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"?search={Uri.EscapeDataString(search)}";
            }

            var result = await _httpClient.GetFromJsonAsync<IReadOnlyList<WorkflowDto>>(url);
            return result ?? Array.Empty<WorkflowDto>();
        }

        public async Task<PaginatedResult<WorkflowDto>> GetPagedWorkflowsAsync(int page, int pageSize, string? search = null)
        {
            var url = $"api/workflows/paged?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search)}";
            }

            var result = await _httpClient.GetFromJsonAsync<PaginatedResult<WorkflowDto>>(url);
            return result ?? new PaginatedResult<WorkflowDto>();
        }

        public Task<WorkflowDto?> GetByIdAsync(Guid id) =>
            _httpClient.GetFromJsonAsync<WorkflowDto>($"api/workflows/{id}");

        public async Task<WorkflowDto?> CreateAsync(UpsertWorkflowRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/workflows", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorkflowDto>();
        }

        public async Task<WorkflowDto?> UpdateAsync(Guid id, UpsertWorkflowRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/workflows/{id}", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorkflowDto>();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/workflows/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<WorkflowVersionDto>> GetVersionsAsync(Guid workflowId)
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<WorkflowVersionDto>>($"api/workflows/{workflowId}/versions");
            return result ?? Array.Empty<WorkflowVersionDto>();
        }

        public async Task<WorkflowDto?> RestoreVersionAsync(Guid workflowId, Guid versionId)
        {
            var response = await _httpClient.PostAsync($"api/workflows/{workflowId}/restore/{versionId}", null);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<WorkflowDto>();
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid workflowId)
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<AuditLogDto>>($"api/workflows/{workflowId}/audit");
            return result ?? Array.Empty<AuditLogDto>();
        }
    }
}
