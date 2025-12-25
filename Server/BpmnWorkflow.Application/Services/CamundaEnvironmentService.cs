using BpmnWorkflow.Application.DTOs.Camunda;
using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Services
{
    public class CamundaEnvironmentService : ICamundaEnvironmentService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CamundaEnvironmentService> _logger;

        public CamundaEnvironmentService(IApplicationDbContext context, ILogger<CamundaEnvironmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<CamundaEnvironmentDto>> GetAllAsync()
        {
            return await _context.CamundaEnvironments
                .Select(e => MapToDto(e))
                .ToListAsync();
        }

        public async Task<CamundaEnvironmentDto?> GetByIdAsync(Guid id)
        {
            var env = await _context.CamundaEnvironments.FindAsync(id);
            return env != null ? MapToDto(env) : null;
        }

        public async Task<CamundaEnvironmentDto> CreateAsync(CamundaEnvironmentUpsertDto dto)
        {
            var env = new CamundaEnvironment
            {
                Name = dto.Name,
                BaseUrl = dto.BaseUrl,
                Username = dto.Username,
                Password = dto.Password,
                ZeebeGatewayUrl = dto.ZeebeGatewayUrl,
                Description = dto.Description,
                IsActive = !await _context.CamundaEnvironments.AnyAsync() // First one is active
            };

            _context.CamundaEnvironments.Add(env);
            await _context.SaveChangesAsync();
            return MapToDto(env);
        }

        public async Task<CamundaEnvironmentDto?> UpdateAsync(Guid id, CamundaEnvironmentUpsertDto dto)
        {
            var env = await _context.CamundaEnvironments.FindAsync(id);
            if (env == null) return null;

            env.Name = dto.Name;
            env.BaseUrl = dto.BaseUrl;
            env.Username = dto.Username;
            env.Password = dto.Password;
            env.ZeebeGatewayUrl = dto.ZeebeGatewayUrl;
            env.Description = dto.Description;

            await _context.SaveChangesAsync();
            return MapToDto(env);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var env = await _context.CamundaEnvironments.FindAsync(id);
            if (env == null) return false;

            if (env.IsActive)
            {
                // Can't delete active one if it's the last one, 
                // but if there are others, activate another one?
                // For simplicity, just prevent deletion of active one for now.
                var count = await _context.CamundaEnvironments.CountAsync();
                if (count > 1)
                {
                    _logger.LogWarning("Attempted to delete active environment {Id}", id);
                    return false;
                }
            }

            _context.CamundaEnvironments.Remove(env);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetActiveAsync(Guid id)
        {
            var env = await _context.CamundaEnvironments.FindAsync(id);
            if (env == null) return false;

            // Deactivate all
            await _context.CamundaEnvironments
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsActive, false));

            // Activate target
            env.IsActive = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CamundaEnvironmentDto?> GetActiveAsync()
        {
            var env = await _context.CamundaEnvironments.FirstOrDefaultAsync(e => e.IsActive);
            return env != null ? MapToDto(env) : null;
        }

        private static CamundaEnvironmentDto MapToDto(CamundaEnvironment env)
        {
            return new CamundaEnvironmentDto
            {
                Id = env.Id,
                Name = env.Name,
                BaseUrl = env.BaseUrl,
                Username = env.Username,
                Password = env.Password,
                IsActive = env.IsActive,
                CreatedAt = env.CreatedAt,
                ZeebeGatewayUrl = env.ZeebeGatewayUrl,
                Description = env.Description
            };
        }
    }
}
