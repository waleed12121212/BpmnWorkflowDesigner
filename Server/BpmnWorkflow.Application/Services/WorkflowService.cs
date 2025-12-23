using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Client.Models;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BpmnWorkflow.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public WorkflowService(IApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IEnumerable<WorkflowDto>> GetAllAsync(string? search = null)
        {
            var query = _context.Workflows.AsNoTracking().Where(w => !w.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.Name.Contains(search) || 
                                       (w.Description != null && w.Description.Contains(search)));
            }

            return await query
                .OrderByDescending(w => w.UpdatedAt)
                .Select(w => new WorkflowDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    BpmnXml = w.BpmnXml,
                    SvgPreview = w.SvgPreview,
                    OwnerId = w.OwnerId,
                    OwnerName = w.Owner != null ? w.Owner.FirstName + " " + w.Owner.LastName : "Unknown",
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    IsPublished = w.IsPublished,
                    Version = w.Version
                })
                .ToListAsync();
        }

        public async Task<PaginatedResult<WorkflowDto>> GetPaginatedAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.Workflows.AsNoTracking().Where(w => !w.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(w => w.Name.Contains(search) || 
                                       (w.Description != null && w.Description.Contains(search)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(w => w.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new WorkflowDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    BpmnXml = w.BpmnXml,
                    SvgPreview = w.SvgPreview,
                    OwnerId = w.OwnerId,
                    OwnerName = w.Owner != null ? w.Owner.FirstName + " " + w.Owner.LastName : "Unknown",
                    CreatedAt = w.CreatedAt,
                    UpdatedAt = w.UpdatedAt,
                    IsPublished = w.IsPublished,
                    Version = w.Version
                })
                .ToListAsync();

            return new PaginatedResult<WorkflowDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<WorkflowDto?> GetByIdAsync(Guid id)
        {
            var workflow = await _context.Workflows
                .Include(w => w.Owner)
                .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);

            if (workflow == null) return null;

            return new WorkflowDto
            {
                Id = workflow.Id,
                Name = workflow.Name,
                Description = workflow.Description,
                BpmnXml = workflow.BpmnXml,
                SvgPreview = workflow.SvgPreview,
                OwnerId = workflow.OwnerId,
                OwnerName = workflow.Owner != null ? workflow.Owner.FirstName + " " + workflow.Owner.LastName : "Unknown",
                CreatedAt = workflow.CreatedAt,
                UpdatedAt = workflow.UpdatedAt,
                IsPublished = workflow.IsPublished,
                Version = workflow.Version
            };
        }

        public async Task<WorkflowDto> CreateAsync(UpsertWorkflowRequest request, Guid userId)
        {
            var newWorkflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                BpmnXml = request.BpmnXml ?? string.Empty,
                SvgPreview = request.SvgPreview,
                OwnerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPublished = false,
                IsDeleted = false,
                Version = 1
            };

            _context.Workflows.Add(newWorkflow);
            
            // Save initial version
            _context.WorkflowVersions.Add(new WorkflowVersion
            {
                Id = Guid.NewGuid(),
                WorkflowId = newWorkflow.Id,
                BpmnXml = newWorkflow.BpmnXml,
                VersionNumber = 1,
                CreatedAt = DateTime.UtcNow,
                Comment = "Initial creation"
            });

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Create", "Workflow", newWorkflow.Id, $"Created workflow: {newWorkflow.Name}");

            var owner = await _context.Users.FindAsync(userId);

            return new WorkflowDto
            {
                Id = newWorkflow.Id,
                Name = newWorkflow.Name,
                Description = newWorkflow.Description,
                BpmnXml = newWorkflow.BpmnXml,
                SvgPreview = newWorkflow.SvgPreview,
                OwnerId = newWorkflow.OwnerId,
                OwnerName = owner != null ? owner.FirstName + " " + owner.LastName : "Unknown",
                CreatedAt = newWorkflow.CreatedAt,
                UpdatedAt = newWorkflow.UpdatedAt,
                IsPublished = newWorkflow.IsPublished,
                Version = newWorkflow.Version
            };
        }

        public async Task<WorkflowDto?> UpdateAsync(Guid id, UpsertWorkflowRequest request, Guid userId)
        {
            var existingWorkflow = await _context.Workflows.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
            if (existingWorkflow == null) return null;

            var changes = new List<string>();
            if (existingWorkflow.Name != request.Name) changes.Add($"Name: '{existingWorkflow.Name}' -> '{request.Name}'");
            if (existingWorkflow.Description != request.Description) changes.Add("Description modified");
            changes.Add($"Version increased to {existingWorkflow.Version}");

            existingWorkflow.Name = request.Name;
            existingWorkflow.Description = request.Description;
            if (request.BpmnXml != null) existingWorkflow.BpmnXml = request.BpmnXml;
            existingWorkflow.SvgPreview = request.SvgPreview;
            existingWorkflow.UpdatedAt = DateTime.UtcNow;
            
            // Auto-increment version
            existingWorkflow.Version++;

            // Save history version
            _context.WorkflowVersions.Add(new WorkflowVersion
            {
                Id = Guid.NewGuid(),
                WorkflowId = existingWorkflow.Id,
                BpmnXml = existingWorkflow.BpmnXml,
                VersionNumber = existingWorkflow.Version,
                CreatedAt = DateTime.UtcNow,
                Comment = "Updated via editor"
            });

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Update", "Workflow", existingWorkflow.Id, string.Join(", ", changes));

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.Id == id);
            if (workflow == null) return false;

            workflow.IsDeleted = true;
            workflow.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Delete", "Workflow", id, $"Deleted workflow: {workflow.Name}");

            return true;
        }

        public async Task<IEnumerable<WorkflowVersionDto>> GetVersionsAsync(Guid workflowId)
        {
            return await _context.WorkflowVersions
                .AsNoTracking()
                .Where(v => v.WorkflowId == workflowId)
                .OrderByDescending(v => v.VersionNumber)
                .Select(v => new WorkflowVersionDto
                {
                    Id = v.Id,
                    VersionNumber = v.VersionNumber,
                    CreatedAt = v.CreatedAt,
                    Comment = v.Comment,
                    BpmnXml = v.BpmnXml
                })
                .ToListAsync();
        }

        public async Task<WorkflowDto?> RestoreVersionAsync(Guid workflowId, Guid versionId, Guid userId)
        {
            var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.Id == workflowId && !w.IsDeleted);
            var version = await _context.WorkflowVersions.FirstOrDefaultAsync(v => v.Id == versionId && v.WorkflowId == workflowId);

            if (workflow == null || version == null) return null;

            // Restore XML and increment version
            workflow.BpmnXml = version.BpmnXml;
            workflow.Version++;
            workflow.UpdatedAt = DateTime.UtcNow;

            // Save this restoration as a NEW version
            _context.WorkflowVersions.Add(new WorkflowVersion
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                BpmnXml = workflow.BpmnXml,
                VersionNumber = workflow.Version,
                CreatedAt = DateTime.UtcNow,
                Comment = $"Restored from Version {version.VersionNumber}"
            });

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(userId, "Restore", "Workflow", workflow.Id, $"Restored version {version.VersionNumber} as new version {workflow.Version}");

            return await GetByIdAsync(workflowId);
        }
        
        // ========== Camunda Integration Methods ==========
        
        public async Task<WorkflowDto?> GetWorkflowByIdAsync(Guid id)
        {
            return await GetByIdAsync(id);
        }
        
        public async Task UpdateCamundaDeploymentInfoAsync(Guid workflowId, string deploymentId, string processDefinitionId)
        {
            var workflow = await _context.Workflows.FirstOrDefaultAsync(w => w.Id == workflowId);
            if (workflow == null)
                throw new InvalidOperationException($"Workflow {workflowId} not found");
            
            workflow.CamundaDeploymentId = deploymentId;
            workflow.CamundaProcessDefinitionId = processDefinitionId;
            workflow.IsDeployed = true;
            workflow.LastDeployedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }
    }
}
