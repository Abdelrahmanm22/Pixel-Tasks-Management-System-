using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Models;

namespace Tasks.Repository.Data.Configrations
{
    public class NotificationConfig : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.Url)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.IsRead)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasIndex(x => new { x.RecipientUserId, x.IsRead });

            // All reference FKs are NoAction — Notification reaches AppUser (twice) and
            // WorkTask, all of which are reachable from Corporation, so any cascade would
            // create a multiple-cascade-path cycle on SQL Server. Task deletion clears the
            // task's notifications manually (see TaskController.Delete).
            builder.HasOne(x => x.Recipient)
                .WithMany()
                .HasForeignKey(x => x.RecipientUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Actor)
                .WithMany()
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.WorkTask)
                .WithMany()
                .HasForeignKey(x => x.WorkTaskId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
