using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Client.Services
{
    public interface IFormService
    {
        Task<IEnumerable<FormDefinitionDto>> GetAllAsync(string? search = null);
        Task<FormDefinitionDto?> GetByIdAsync(Guid id);
        Task<FormDefinitionDto?> CreateAsync(UpsertFormRequest request);
        Task<FormDefinitionDto?> UpdateAsync(Guid id, UpsertFormRequest request, string? comment = null);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<FormVersionDto>> GetVersionsAsync(Guid formId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid formId);
    }

    public class FormService : IFormService
    {
        private readonly HttpClient _httpClient;

        public FormService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<FormDefinitionDto>> GetAllAsync(string? search = null)
        {
            var url = "api/forms";
            if (!string.IsNullOrEmpty(search)) url += $"?search={Uri.EscapeDataString(search)}";
            return await _httpClient.GetFromJsonAsync<IEnumerable<FormDefinitionDto>>(url) ?? Array.Empty<FormDefinitionDto>();
        }

        public async Task<FormDefinitionDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<FormDefinitionDto>($"api/forms/{id}");
        }

        public async Task<FormDefinitionDto?> CreateAsync(UpsertFormRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/forms", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<FormDefinitionDto>();
        }

        public async Task<FormDefinitionDto?> UpdateAsync(Guid id, UpsertFormRequest request, string? comment = null)
        {
            var url = $"api/forms/{id}";
            if (!string.IsNullOrEmpty(comment)) url += $"?comment={Uri.EscapeDataString(comment)}";
            var response = await _httpClient.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<FormDefinitionDto>();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/forms/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<FormVersionDto>> GetVersionsAsync(Guid formId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<FormVersionDto>>($"api/forms/{formId}/versions") ?? Array.Empty<FormVersionDto>();
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid formId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<AuditLogDto>>($"api/forms/{formId}/audit-logs") ?? Array.Empty<AuditLogDto>();
        }
    }
}
