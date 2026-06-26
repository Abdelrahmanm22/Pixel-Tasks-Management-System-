using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class CorporationConfig : IEntityTypeConfiguration<Corporation>
    {
        public void Configure(EntityTypeBuilder<Corporation> builder) 
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameAr)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Notes)
                .IsRequired(false)
                .HasMaxLength(5000);
        }
    }
}
