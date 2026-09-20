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

            builder.ComplexProperty(b => b.Amount, price => price.ConfigureMoney("amount"));

            builder.HasOne<User>().WithMany().HasForeignKey(b => b.BidderUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => new { b.AuctionId, b.PlacedAt });
        }
    }
}
