using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class TaskAssignmentConfig : IEntityTypeConfiguration<TaskAssignment>
    {
        public void Configure(EntityTypeBuilder<TaskAssignment> builder)
        {
            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CompletedCount)
                .IsRequired(false);

            builder.Property(x => x.AssignedAt)
                .IsRequired();

            // One user cannot be assigned the same task twice
            builder.HasIndex(x => new { x.WorkTaskId, x.UserId })
                .IsUnique();

            // WorkTask owns its assignments — cascade is safe
            builder.HasOne(x => x.WorkTask)
                .WithMany(t => t.Assignments)
                .HasForeignKey(x => x.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // NoAction: AppUser → TaskAssignment AND WorkTask → TaskAssignment
            // Both paths reach TaskAssignment from a common ancestor (Corporation).
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
