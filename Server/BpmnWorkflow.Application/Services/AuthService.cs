using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Client.Models;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BpmnWorkflow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IApplicationDbContext context, IPasswordService passwordService, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return new AuthResponse { Success = false, Message = "User already exists." };
            }

            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (defaultRole == null)
            {
                defaultRole = new Role { Name = "User", Description = "Default User Role", CanView = true, CanCreate = true, CanEdit = true };
                _context.Roles.Add(defaultRole);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _passwordService.HashPassword(request.Password),
                RoleId = defaultRole.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResponse { Success = true, Message = "User registered successfully." };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                _logger.LogInformation("Login attempt for {Username}. Total users in DB: {Count}", request.Username, userCount);

                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User {Username} not found.", request.Username);
                    return new AuthResponse { Success = false, Message = "Invalid username or password." };
                }

                if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Password mismatch for user {Username}.", request.Username);
                    return new AuthResponse { Success = false, Message = "Invalid username or password." };
                }

                var token = _tokenService.CreateToken(user);

                _logger.LogInformation("User {Username} logged in successfully.", request.Username);

                return new AuthResponse
                {
                    Success = true,
                    Token = token,
                    Username = user.Username,
                    Email = user.Email,
                    Roles = new List<string> { user.Role?.Name ?? "User" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return new AuthResponse 
                { 
                    Success = false, 
                    Message = $"SERVER_ERROR: {ex.Message}" 
                };
            }
        }
    }
}
