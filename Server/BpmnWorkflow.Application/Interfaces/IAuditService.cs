using BpmnWorkflow.Client.Models;
using System;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(Guid userId, string action, string entityType, Guid entityId, string? changes = null);
        Task<IEnumerable<AuditLogDto>> GetWorkflowAuditLogsAsync(Guid workflowId);
    }
}
