using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;
using Tasks.Domain.Models.Identity;

namespace Tasks.Repository.Data.Configrations
{
    public class AppUserConfig : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Gender)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.HasIndex(x => x.UserName).IsUnique();
            builder.HasIndex(x => x.Email).IsUnique();

            // Relationships — nullable FKs (admin users may not belong to any corp/section)
            // NoAction: SQL Server cannot have multiple cascade paths reaching the same table.
            // Corporation → AppUser → WorkTask AND Corporation → WorkTask would create a cycle.
            builder.HasOne(x => x.Corporation)
                .WithMany(c => c.Users)
                .HasForeignKey(x => x.CorporationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Section)
                .WithMany(s => s.Users)
                .HasForeignKey(x => x.SectionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
