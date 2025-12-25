using System.Net;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using BpmnWorkflow.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace BpmnWorkflow.Client.Handlers
{
    public class AuthenticationHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthenticationHeaderHandler(ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (_authenticationStateProvider is ApiAuthenticationStateProvider apiAuthProvider)
                {
                    await apiAuthProvider.NotifyUserLogoutAsync();
                }
            }

            return response;
        }
    }
}
