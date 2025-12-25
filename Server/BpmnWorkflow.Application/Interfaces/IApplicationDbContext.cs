using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BpmnWorkflow.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<Department> Departments { get; }
        DbSet<Workflow> Workflows { get; }
        DbSet<WorkflowVersion> WorkflowVersions { get; }
        DbSet<AuditLog> AuditLogs { get; }
        DbSet<Comment> Comments { get; }
        DbSet<ProcessInstance> ProcessInstances { get; }
        DbSet<CamundaEnvironment> CamundaEnvironments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
