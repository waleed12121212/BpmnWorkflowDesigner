using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BpmnWorkflow.Infrastructure.Data.Configurations
{
    public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
    {
        public void Configure(EntityTypeBuilder<Workflow> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.BpmnXml).IsRequired();
            builder.Property(x => x.SvgPreview).HasColumnType("nvarchar(max)");
            builder.Property(x => x.Tags).HasMaxLength(500);

            builder.HasIndex(x => x.OwnerId);
            builder.HasIndex(x => x.DepartmentId);
            builder.HasIndex(x => x.Name);
            builder.HasIndex(x => x.IsDeleted);

            builder.HasOne(x => x.Owner)
                .WithMany(u => u.Workflows)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany(d => d.Workflows)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

