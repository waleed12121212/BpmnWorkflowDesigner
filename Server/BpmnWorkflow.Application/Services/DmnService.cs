using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Client.Models;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Services
{
    public class DmnService : IDmnService
    {
        private readonly IApplicationDbContext _context;

        public DmnService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DmnDefinitionDto>> GetAllAsync(string? search = null)
        {
            var query = _context.DmnDefinitions.AsNoTracking().Where(d => !d.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search) || (d.Description != null && d.Description.Contains(search)));
            }

            return await query
                .OrderByDescending(d => d.UpdatedAt)
                .Select(d => new DmnDefinitionDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    DmnXml = d.DmnXml,
                    OwnerId = d.OwnerId,
                    OwnerName = d.Owner != null ? d.Owner.FirstName + " " + d.Owner.LastName : "Unknown",
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<DmnDefinitionDto?> GetByIdAsync(Guid id)
        {
            var dmn = await _context.DmnDefinitions
                .Include(d => d.Owner)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (dmn == null) return null;

            return new DmnDefinitionDto
            {
                Id = dmn.Id,
                Name = dmn.Name,
                Description = dmn.Description,
                DmnXml = dmn.DmnXml,
                OwnerId = dmn.OwnerId,
                OwnerName = dmn.Owner != null ? dmn.Owner.FirstName + " " + dmn.Owner.LastName : "Unknown",
                CreatedAt = dmn.CreatedAt,
                UpdatedAt = dmn.UpdatedAt
            };
        }

        public async Task<DmnDefinitionDto> CreateAsync(UpsertDmnRequest request, Guid userId)
        {
            var dmn = new DmnDefinition
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                DmnXml = request.DmnXml,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.DmnDefinitions.Add(dmn);
            await _context.SaveChangesAsync();

            var owner = await _context.Users.FindAsync(userId);

            return new DmnDefinitionDto
            {
                Id = dmn.Id,
                Name = dmn.Name,
                Description = dmn.Description,
                DmnXml = dmn.DmnXml,
                OwnerId = dmn.OwnerId,
                OwnerName = owner != null ? owner.FirstName + " " + owner.LastName : "Unknown",
                CreatedAt = dmn.CreatedAt,
                UpdatedAt = dmn.UpdatedAt
            };
        }

        public async Task<DmnDefinitionDto?> UpdateAsync(Guid id, UpsertDmnRequest request, Guid userId, string? versionComment = null)
        {
            var dmn = await _context.DmnDefinitions.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            if (dmn == null) return null;

            // Save version before updating
            var currentVersionCount = await _context.DmnVersions.CountAsync(v => v.DmnId == id);
            var version = new DmnVersion
            {
                Id = Guid.NewGuid(),
                DmnId = id,
                DmnXml = dmn.DmnXml,
                VersionNumber = currentVersionCount + 1,
                CreatedAt = DateTime.UtcNow,
                Comment = versionComment ?? "Auto-saved version",
                CreatedBy = userId.ToString()
            };
            _context.DmnVersions.Add(version);

            dmn.Name = request.Name;
            dmn.Description = request.Description;
            dmn.DmnXml = request.DmnXml;
            dmn.UpdatedAt = DateTime.UtcNow;

            // Add Audit Log
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "Update",
                EntityType = "DMN",
                EntityId = id,
                Timestamp = DateTime.UtcNow,
                Changes = $"Updated DMN: {dmn.Name}"
            });

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var dmn = await _context.DmnDefinitions.FirstOrDefaultAsync(d => d.Id == id);
            if (dmn == null) return false;

            dmn.IsDeleted = true;
            
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "Delete",
                EntityType = "DMN",
                EntityId = id,
                Timestamp = DateTime.UtcNow,
                Changes = $"Deleted DMN: {dmn.Name}"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DmnVersionDto>> GetVersionsAsync(Guid dmnId)
        {
            return await _context.DmnVersions
                .AsNoTracking()
                .Where(v => v.DmnId == dmnId)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new DmnVersionDto
                {
                    Id = v.Id,
                    DmnId = v.DmnId,
                    DmnXml = v.DmnXml,
                    VersionNumber = v.VersionNumber,
                    CreatedAt = v.CreatedAt,
                    CreatedBy = v.CreatedBy,
                    Comment = v.Comment
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid dmnId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityType == "DMN" && a.EntityId == dmnId)
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    UserId = a.UserId,
                    UserName = a.User != null ? a.User.Username : "System",
                    Timestamp = a.Timestamp,
                    Changes = a.Changes
                })
                .ToListAsync();
        }
    }
}
