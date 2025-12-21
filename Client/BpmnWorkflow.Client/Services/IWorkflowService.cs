using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Client.Services
{
    public interface IWorkflowService
    {
        Task<IReadOnlyList<WorkflowDto>> GetWorkflowsAsync(string? search = null);
        Task<PaginatedResult<WorkflowDto>> GetPagedWorkflowsAsync(int page, int pageSize, string? search = null);
        Task<WorkflowDto?> GetByIdAsync(Guid id);
        Task<WorkflowDto?> CreateAsync(UpsertWorkflowRequest request);
        Task<WorkflowDto?> UpdateAsync(Guid id, UpsertWorkflowRequest request);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<WorkflowVersionDto>> GetVersionsAsync(Guid workflowId);
        Task<WorkflowDto?> RestoreVersionAsync(Guid workflowId, Guid versionId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid workflowId);
    }
}


