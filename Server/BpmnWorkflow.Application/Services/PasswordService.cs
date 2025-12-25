using BpmnWorkflow.Application.Interfaces;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace BpmnWorkflow.Application.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService>? _logger;

        public PasswordService(ILogger<PasswordService>? logger = null)
        {
            _logger = logger;
        }

        public string HashPassword(string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.HashPassword(password);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error hashing password");
                throw new InvalidOperationException("Failed to hash password", ex);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    _logger?.LogWarning("Attempted to verify password against null or empty hash");
                    return false;
                }
                
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verifying password. Hash: {Hash}", hashedPassword?.Substring(0, Math.Min(10, hashedPassword?.Length ?? 0)));
                return false;
            }
        }
    }
}
