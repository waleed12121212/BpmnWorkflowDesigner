using BpmnWorkflow.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface IDmnService
    {
        Task<IEnumerable<DmnDefinitionDto>> GetAllAsync(string? search = null);
        Task<DmnDefinitionDto?> GetByIdAsync(Guid id);
        Task<DmnDefinitionDto> CreateAsync(UpsertDmnRequest request, Guid userId);
        Task<DmnDefinitionDto?> UpdateAsync(Guid id, UpsertDmnRequest request, Guid userId, string? versionComment = null);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<IEnumerable<DmnVersionDto>> GetVersionsAsync(Guid dmnId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid dmnId);
    }
}
