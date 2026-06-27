using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class SectionConfig : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);
            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.Email)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.Fax)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(x => x.Phone)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(x => x.Address)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.Telex)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(x => x.Notes)
                .IsRequired(false)
                .HasMaxLength(5000);

            // Relationships
            builder.HasOne(x => x.Corporation)
                .WithMany(c => c.Sections)
                .HasForeignKey(x => x.CorporationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
