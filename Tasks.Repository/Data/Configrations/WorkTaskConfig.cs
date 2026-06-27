using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class WorkTaskConfig : IEntityTypeConfiguration<WorkTask>
    {
        public void Configure(EntityTypeBuilder<WorkTask> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.Description)
                .IsRequired(false)
                .HasMaxLength(5000);

            builder.Property(x => x.Notes)
                .IsRequired(false)
                .HasMaxLength(5000);

            builder.Property(x => x.RequestDate)
                .IsRequired();

            builder.Property(x => x.DueDate)
                .IsRequired();

            builder.Property(x => x.Priority)
                .IsRequired();

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.TargetCount)
                .IsRequired(false);

            // Relationships
            // TaskType owns the task — cascade is safe (no cycle path through TaskType)
            builder.HasOne(x => x.TaskType)
                .WithMany(t => t.Tasks)
                .HasForeignKey(x => x.TaskTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // NoAction for the remaining 3 FKs — SQL Server forbids multiple cascade paths:
            //   Corporation → WorkTask (direct) AND Corporation → Section → WorkTask
            //   Corporation → AppUser → WorkTask  — all three reach WorkTask from Corporation.
            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Corporation)
                .WithMany()
                .HasForeignKey(x => x.CorporationId)
                .OnDelete(DeleteBehavior.NoAction);

            // SectionId is nullable — task may target the whole corporation
            builder.HasOne(x => x.Section)
                .WithMany()
                .HasForeignKey(x => x.SectionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
