using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using BpmnWorkflow.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace BpmnWorkflow.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ApiAuthenticationStateProvider _authStateProvider;

        private const string TokenStorageKey = "authToken";

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = (ApiAuthenticationStateProvider)authStateProvider;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    return errorResponse ?? new AuthResponse { Success = false, Message = response.ReasonPhrase ?? "Unknown error" };
                }
                catch
                {
                    return new AuthResponse { Success = false, Message = $"Server returned {response.StatusCode}" };
                }
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse is { Success: true, Token: not null })
            {
                await _authStateProvider.NotifyUserAuthenticationAsync(authResponse.Token);
            }

            return authResponse ?? new AuthResponse { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    return errorResponse ?? new AuthResponse { Success = false, Message = "Registration failed" };
                }
                catch
                {
                    return new AuthResponse { Success = false, Message = $"Registration failed with {response.StatusCode}" };
                }
            }
            return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { Success = false, Message = "Failed to deserialize response" };
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(TokenStorageKey);
            await _authStateProvider.NotifyUserLogoutAsync();
        }
    }
}


