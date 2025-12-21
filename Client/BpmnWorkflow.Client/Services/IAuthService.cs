using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Client.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task LogoutAsync();
    }
}


