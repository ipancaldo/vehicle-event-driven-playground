using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class AuctionBidConfiguration : IEntityTypeConfiguration<AuctionBid>
    {
        public void Configure(EntityTypeBuilder<AuctionBid> builder)
        {
            builder.ToTable("auction_bids");
            builder.HasKey(b => b.Id);
            // Domain-assigned Guid, not store-generated — see ListingPriceChangeConfiguration for
            // why this matters: a real PlaceBid command loads an already-tracked Auction and appends
            // to its Bids collection, which is exactly the shape that trips EF's default convention.
            builder.Property(b => b.Id).ValueGeneratedNever();

            builder.ComplexProperty(b => b.Amount, price => price.ConfigureMoney("amount"));

            builder.HasOne<User>().WithMany().HasForeignKey(b => b.BidderUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => new { b.AuctionId, b.PlacedAt });
        }
    }
}
