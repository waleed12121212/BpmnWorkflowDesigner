using BpmnWorkflow.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface IFormService
    {
        Task<IEnumerable<FormDefinitionDto>> GetAllAsync(string? search = null);
        Task<FormDefinitionDto?> GetByIdAsync(Guid id);
        Task<FormDefinitionDto> CreateAsync(UpsertFormRequest request, Guid userId);
        Task<FormDefinitionDto?> UpdateAsync(Guid id, UpsertFormRequest request, Guid userId, string? versionComment = null);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<IEnumerable<FormVersionDto>> GetVersionsAsync(Guid formId);
        Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(Guid formId);
    }
}
