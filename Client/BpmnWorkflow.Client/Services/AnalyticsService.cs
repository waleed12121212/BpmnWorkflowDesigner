using BpmnWorkflow.Client.Models;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BpmnWorkflow.Client.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardAnalyticsDto?> GetDashboardAnalyticsAsync();
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly HttpClient _httpClient;

        public AnalyticsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DashboardAnalyticsDto?> GetDashboardAnalyticsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DashboardAnalyticsDto>("api/analytics/dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching analytics: {ex.Message}");
                return null;
            }
        }
    }
}
