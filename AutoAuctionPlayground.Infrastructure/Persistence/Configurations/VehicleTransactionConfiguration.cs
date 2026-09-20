using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Transactions;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class VehicleTransactionConfiguration : IEntityTypeConfiguration<VehicleTransaction>
    {
        public void Configure(EntityTypeBuilder<VehicleTransaction> builder)
        {
            builder.ToTable("vehicle_transactions");
            builder.HasKey(t => t.Id);

            builder.ComplexProperty(t => t.FinalPrice, price => price.ConfigureMoney("final_price"));

            builder.Property(t => t.Kind).HasConversion<string>().HasMaxLength(20);

            builder.HasOne<VehicleListing>().WithMany().HasForeignKey(t => t.VehicleListingId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Auction>().WithMany().HasForeignKey(t => t.AuctionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>().WithMany().HasForeignKey(t => t.BuyerUserId).OnDelete(DeleteBehavior.Restrict);

            // Double-sale guard: exactly one completed transaction per listing, enforced by the
            // database. Two racing buyers both pass the domain checks; only one insert succeeds.
            builder.HasIndex(t => t.VehicleListingId)
                   .IsUnique()
                   .HasDatabaseName("ix_vehicle_transactions_one_per_listing");

            // One outcome per auction.
            builder.HasIndex(t => t.AuctionId)
                   .IsUnique()
                   .HasFilter("auction_id IS NOT NULL")
                   .HasDatabaseName("ix_vehicle_transactions_one_per_auction");
        }
    }
}
