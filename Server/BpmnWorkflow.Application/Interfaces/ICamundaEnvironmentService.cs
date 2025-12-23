using BpmnWorkflow.Application.DTOs.Camunda;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface ICamundaEnvironmentService
    {
        Task<List<CamundaEnvironmentDto>> GetAllAsync();
        Task<CamundaEnvironmentDto?> GetByIdAsync(Guid id);
        Task<CamundaEnvironmentDto> CreateAsync(CamundaEnvironmentUpsertDto dto);
        Task<CamundaEnvironmentDto?> UpdateAsync(Guid id, CamundaEnvironmentUpsertDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SetActiveAsync(Guid id);
        Task<CamundaEnvironmentDto?> GetActiveAsync();
    }
}
