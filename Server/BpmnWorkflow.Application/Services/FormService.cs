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
    public class FormService : IFormService
    {
        private readonly IApplicationDbContext _context;

        public FormService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FormDefinitionDto>> GetAllAsync(string? search = null)
        {
            var query = _context.FormDefinitions.AsNoTracking().Where(f => !f.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => f.Name.Contains(search) || (f.Description != null && f.Description.Contains(search)));
            }

            return await query
                .OrderByDescending(f => f.UpdatedAt)
                .Select(f => new FormDefinitionDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    SchemaJson = f.SchemaJson,
                    OwnerId = f.OwnerId,
                    OwnerName = f.Owner != null ? f.Owner.FirstName + " " + f.Owner.LastName : "Unknown",
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<FormDefinitionDto?> GetByIdAsync(Guid id)
        {
            var form = await _context.FormDefinitions
                .Include(f => f.Owner)
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

            if (form == null) return null;

            return new FormDefinitionDto
            {
                Id = form.Id,
                Name = form.Name,
                Description = form.Description,
                SchemaJson = form.SchemaJson,
                OwnerId = form.OwnerId,
                OwnerName = form.Owner != null ? form.Owner.FirstName + " " + form.Owner.LastName : "Unknown",
                CreatedAt = form.CreatedAt,
                UpdatedAt = form.UpdatedAt
            };
        }

        public async Task<FormDefinitionDto> CreateAsync(UpsertFormRequest request, Guid userId)
        {
            var form = new FormDefinition
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                SchemaJson = request.SchemaJson,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.FormDefinitions.Add(form);
            await _context.SaveChangesAsync();

            var owner = await _context.Users.FindAsync(userId);

            return new FormDefinitionDto
            {
                Id = form.Id,
                Name = form.Name,
                Description = form.Description,
                SchemaJson = form.SchemaJson,
                OwnerId = form.OwnerId,
                OwnerName = owner != null ? owner.FirstName + " " + owner.LastName : "Unknown",
                CreatedAt = form.CreatedAt,
                UpdatedAt = form.UpdatedAt
            };
        }

        public async Task<FormDefinitionDto?> UpdateAsync(Guid id, UpsertFormRequest request, Guid userId, string? versionComment = null)
        {
            var form = await _context.FormDefinitions.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
            if (form == null) return null;

            // Save version before updating
            var currentVersionCount = await _context.FormVersions.CountAsync(v => v.FormId == id);
            var version = new FormVersion
            {
                Id = Guid.NewGuid(),
                FormId = id,
                SchemaJson = form.SchemaJson,
                VersionNumber = currentVersionCount + 1,
                CreatedAt = DateTime.UtcNow,
                Comment = versionComment ?? "Auto-saved version",
                CreatedBy = userId.ToString()
            };
            _context.FormVersions.Add(version);

            form.Name = request.Name;
            form.Description = request.Description;
            form.SchemaJson = request.SchemaJson;
            form.UpdatedAt = DateTime.UtcNow;

            // Add Audit Log
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "Update",
                EntityType = "Form",
                EntityId = id,
                Timestamp = DateTime.UtcNow,
                Changes = $"Updated form: {form.Name}"
            });

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var form = await _context.FormDefinitions.FirstOrDefaultAsync(f => f.Id == id);
            if (form == null) return false;

            form.IsDeleted = true;
            
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = "Delete",
                EntityType = "Form",
                EntityId = id,
                Timestamp = DateTime.UtcNow,
                Changes = $"Deleted form: {form.Name}"
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FormVersionDto>> GetVersionsAsync(Guid formId)
        {
            return await _context.FormVersions
                .AsNoTracking()
                .Where(v => v.FormId == formId)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new FormVersionDto
                {
                    Id = v.Id,
                    FormId = v.FormId,
                    SchemaJson = v.SchemaJson,
                    VersionNumber = v.VersionNumber,
                    CreatedAt = v.CreatedAt,
                    CreatedBy = v.CreatedBy,
                    Comment = v.Comment
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid formId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityType == "Form" && a.EntityId == formId)
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
