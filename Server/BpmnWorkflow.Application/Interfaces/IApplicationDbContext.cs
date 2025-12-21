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
        DbSet<FormDefinition> FormDefinitions { get; }
        DbSet<FormVersion> FormVersions { get; }
        DbSet<DmnDefinition> DmnDefinitions { get; }
        DbSet<DmnVersion> DmnVersions { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
