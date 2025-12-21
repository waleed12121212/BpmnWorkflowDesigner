using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Client.Services
{
    public class CommentService : ICommentService
    {
        private readonly HttpClient _httpClient;

        public CommentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsAsync(Guid workflowId)
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<CommentDto>>($"api/comments/workflow/{workflowId}");
            return result ?? Array.Empty<CommentDto>();
        }

        public async Task<CommentDto?> AddCommentAsync(Guid workflowId, string text, string? elementId = null)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/comments/workflow/{workflowId}", new { Text = text, ElementId = elementId });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CommentDto>();
        }

        public async Task<IEnumerable<CommentDto>> GetFormCommentsAsync(Guid formId)
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<CommentDto>>($"api/comments/form/{formId}");
            return result ?? Array.Empty<CommentDto>();
        }

        public async Task<CommentDto?> AddFormCommentAsync(Guid formId, string text)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/comments/form/{formId}", new { Text = text });
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CommentDto>();
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId)
        {
            var response = await _httpClient.DeleteAsync($"api/comments/{commentId}");
            return response.IsSuccessStatusCode;
        }
    }
}
