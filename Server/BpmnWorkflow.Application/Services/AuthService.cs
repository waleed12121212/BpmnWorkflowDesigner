using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Client.Models;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BpmnWorkflow.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public AuthService(IApplicationDbContext context, IPasswordService passwordService, ITokenService tokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
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
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthResponse { Success = false, Message = "Invalid username or password." };
            }

            var token = _tokenService.CreateToken(user);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Roles = new List<string> { user.Role?.Name ?? "User" }
            };
        }
    }
}
