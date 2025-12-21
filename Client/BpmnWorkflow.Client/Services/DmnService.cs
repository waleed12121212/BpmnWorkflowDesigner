using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Client.Services
{
    public interface IDmnService
    {
        Task<IEnumerable<DmnDefinitionDto>> GetAllAsync(string? search = null);
        Task<DmnDefinitionDto?> GetByIdAsync(Guid id);
        Task<DmnDefinitionDto?> CreateAsync(UpsertDmnRequest request);
        Task<DmnDefinitionDto?> UpdateAsync(Guid id, UpsertDmnRequest request, string? comment = null);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<DmnVersionDto>> GetVersionsAsync(Guid dmnId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid dmnId);
    }

    public class DmnService : IDmnService
    {
        private readonly HttpClient _httpClient;

        public DmnService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<DmnDefinitionDto>> GetAllAsync(string? search = null)
        {
            var url = "api/dmn";
            if (!string.IsNullOrEmpty(search)) url += $"?search={Uri.EscapeDataString(search)}";
            return await _httpClient.GetFromJsonAsync<IEnumerable<DmnDefinitionDto>>(url) ?? Array.Empty<DmnDefinitionDto>();
        }

        public async Task<DmnDefinitionDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<DmnDefinitionDto>($"api/dmn/{id}");
        }

        public async Task<DmnDefinitionDto?> CreateAsync(UpsertDmnRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/dmn", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<DmnDefinitionDto>();
        }

        public async Task<DmnDefinitionDto?> UpdateAsync(Guid id, UpsertDmnRequest request, string? comment = null)
        {
            var url = $"api/dmn/{id}";
            if (!string.IsNullOrEmpty(comment)) url += $"?comment={Uri.EscapeDataString(comment)}";
            var response = await _httpClient.PutAsJsonAsync(url, request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<DmnDefinitionDto>();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/dmn/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<DmnVersionDto>> GetVersionsAsync(Guid dmnId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DmnVersionDto>>($"api/dmn/{dmnId}/versions") ?? Array.Empty<DmnVersionDto>();
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid dmnId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<AuditLogDto>>($"api/dmn/{dmnId}/audit-logs") ?? Array.Empty<AuditLogDto>();
        }
    }
}
