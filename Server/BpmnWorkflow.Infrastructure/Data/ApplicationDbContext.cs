using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BpmnWorkflow.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Workflow> Workflows => Set<Workflow>();
        public DbSet<WorkflowVersion> WorkflowVersions => Set<WorkflowVersion>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
        public DbSet<FormVersion> FormVersions => Set<FormVersion>();
        public DbSet<DmnDefinition> DmnDefinitions => Set<DmnDefinition>();
        public DbSet<DmnVersion> DmnVersions => Set<DmnVersion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}

