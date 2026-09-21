using AutoAuctionPlayground.Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedNever();

            builder.Property(m => m.MessageType).HasConversion<string>().HasMaxLength(50);
            builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(m => m.Payload).IsRequired();
            builder.Property(m => m.LockedBy).HasMaxLength(200);

            builder.HasIndex(m => new { m.MessageType, m.Status })
                   .HasDatabaseName("ix_outbox_messages_type_status");

            // Serves OutboxPublisher's claim query: WHERE status = Pending AND (locked_until_utc IS
            // NULL OR locked_until_utc < now()) ORDER BY created_at_utc — no MessageType filter, so
            // the index above doesn't serve it.
            builder.HasIndex(m => new { m.Status, m.LockedUntilUtc, m.CreatedAtUtc })
                   .HasDatabaseName("ix_outbox_messages_claim");
        }
    }
}
