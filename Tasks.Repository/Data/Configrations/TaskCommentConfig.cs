using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class TaskCommentConfig : IEntityTypeConfiguration<TaskComment>
    {
        public void Configure(EntityTypeBuilder<TaskComment> builder)
        {
            builder.Property(x => x.Content)
                .IsRequired(false)
                .HasMaxLength(5000);

            builder.Property(x => x.FileUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Relationships
            // WorkTask owns its comments — cascade is safe
            builder.HasOne(x => x.WorkTask)
                .WithMany(t => t.Comments)
                .HasForeignKey(x => x.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // NoAction: AppUser → TaskComment AND WorkTask → TaskComment
            // Both paths reachable from Corporation — SQL Server cycle.
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // NoAction: WorkTask→Assignment→Comment AND WorkTask→Comment would be a second cascade path.
            builder.HasOne(x => x.TaskAssignment)
                .WithMany()
                .HasForeignKey(x => x.TaskAssignmentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
