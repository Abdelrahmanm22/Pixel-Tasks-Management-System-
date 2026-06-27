using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class TaskPointConfig : IEntityTypeConfiguration<TaskPoint>
    {
        public void Configure(EntityTypeBuilder<TaskPoint> builder)
        {
            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Order)
                .IsRequired();

            // Relationship
            builder.HasOne(x => x.WorkTask)
                .WithMany(t => t.Points)
                .HasForeignKey(x => x.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
