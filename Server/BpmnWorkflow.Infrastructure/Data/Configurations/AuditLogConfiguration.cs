using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BpmnWorkflow.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
            builder.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Changes).HasColumnType("nvarchar(max)");
            builder.Property(x => x.IpAddress).HasMaxLength(50);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Timestamp);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

