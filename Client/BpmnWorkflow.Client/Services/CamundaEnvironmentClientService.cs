using BpmnWorkflow.Client.Models;
using System.Net.Http.Json;

namespace BpmnWorkflow.Client.Services
{
    public class CamundaEnvironmentClientService
    {
        private readonly HttpClient _httpClient;

        public CamundaEnvironmentClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CamundaEnvironmentDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<CamundaEnvironmentDto>>("api/camundaenvironment") ?? new();
        }

        public async Task<CamundaEnvironmentDto?> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<CamundaEnvironmentDto>($"api/camundaenvironment/{id}");
        }

        public async Task<CamundaEnvironmentDto?> CreateAsync(CamundaEnvironmentUpsertDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/camundaenvironment", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CamundaEnvironmentDto>();
            }
            return null;
        }

        public async Task<CamundaEnvironmentDto?> UpdateAsync(Guid id, CamundaEnvironmentUpsertDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/camundaenvironment/{id}", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CamundaEnvironmentDto>();
            }
            return null;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/camundaenvironment/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateAsync(Guid id)
        {
            var response = await _httpClient.PostAsync($"api/camundaenvironment/{id}/activate", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<CamundaEnvironmentDto?> GetActiveAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CamundaEnvironmentDto>("api/camundaenvironment/active");
            }
            catch
            {
                return null;
            }
        }
    }
}
