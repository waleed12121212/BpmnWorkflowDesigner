using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BpmnWorkflow.Infrastructure.Data.Configurations
{
    public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
    {
        public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.BpmnXml)
                .IsRequired();

            builder.HasOne(x => x.Workflow)
                .WithMany()
                .HasForeignKey(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
