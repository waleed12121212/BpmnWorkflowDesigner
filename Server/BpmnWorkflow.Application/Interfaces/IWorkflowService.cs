using BpmnWorkflow.Client.Models;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface IWorkflowService
    {
        Task<IEnumerable<WorkflowDto>> GetAllAsync(string? search = null);
        Task<PaginatedResult<WorkflowDto>> GetPaginatedAsync(int page, int pageSize, string? search = null);
        Task<WorkflowDto?> GetByIdAsync(Guid id);
        Task<WorkflowDto> CreateAsync(UpsertWorkflowRequest request, Guid userId);
        Task<WorkflowDto?> UpdateAsync(Guid id, UpsertWorkflowRequest request, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<IEnumerable<WorkflowVersionDto>> GetVersionsAsync(Guid workflowId);
        Task<WorkflowDto?> RestoreVersionAsync(Guid workflowId, Guid versionId, Guid userId);
    }
}
