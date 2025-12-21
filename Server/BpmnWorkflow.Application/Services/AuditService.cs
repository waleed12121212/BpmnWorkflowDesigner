using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Domain.Entities;
using BpmnWorkflow.Client.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IApplicationDbContext _context;

        public AuditService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(Guid userId, string action, string entityType, Guid entityId, string? changes = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Changes = changes,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLogDto>> GetWorkflowAuditLogsAsync(Guid workflowId)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(l => l.EntityId == workflowId && l.EntityType == "Workflow")
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new AuditLogDto
                {
                    Id = l.Id,
                    UserName = l.User != null ? l.User.Username : "Unknown",
                    Action = l.Action,
                    EntityType = l.EntityType,
                    Changes = l.Changes,
                    Timestamp = l.Timestamp
                })
                .ToListAsync();
        }
    }
}
