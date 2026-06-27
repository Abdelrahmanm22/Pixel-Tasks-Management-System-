using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class TaskTypeConfig : IEntityTypeConfiguration<TaskType>
    {
        public void Configure(EntityTypeBuilder<TaskType> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.Property(x => x.Category)
                .IsRequired();
        }
    }
}
