using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class TaskPointStatusConfig : IEntityTypeConfiguration<TaskPointStatus>
    {
        public void Configure(EntityTypeBuilder<TaskPointStatus> builder)
        {
            builder.Property(x => x.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CompletedAt)
                .IsRequired(false);

            // One status record per (assignment, point) combination
            builder.HasIndex(x => new { x.TaskAssignmentId, x.TaskPointId })
                .IsUnique();

            // Relationships
            // TaskAssignment owns its point statuses — cascade is safe
            builder.HasOne(x => x.TaskAssignment)
                .WithMany(a => a.PointStatuses)
                .HasForeignKey(x => x.TaskAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // NoAction: WorkTask → TaskAssignment → TaskPointStatus
            //       AND WorkTask → TaskPoint → TaskPointStatus
            // Both paths reach TaskPointStatus from WorkTask — SQL Server cycle.
            builder.HasOne(x => x.TaskPoint)
                .WithMany(p => p.PointStatuses)
                .HasForeignKey(x => x.TaskPointId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
