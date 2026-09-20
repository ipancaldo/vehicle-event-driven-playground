using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            builder.ToTable("auctions");
            builder.HasKey(a => a.Id);

            builder.ComplexProperty(a => a.StartingPrice, price => price.ConfigureMoney("starting_price"));
            builder.ComplexProperty(a => a.HighestBidAmount, price =>
            {
                price.IsRequired(false);
                price.ConfigureMoney("highest_bid");
            });

            builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

            // Optimistic concurrency: EF adds "AND version = @original" to every UPDATE and throws
            // DbUpdateConcurrencyException when 0 rows are affected. The domain increments Version.
            builder.Property(a => a.Version).IsConcurrencyToken();

            builder.HasOne<VehicleListing>().WithMany().HasForeignKey(a => a.VehicleListingId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>().WithMany().HasForeignKey(a => a.SellerUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Company>().WithMany().HasForeignKey(a => a.SellerCompanyId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<User>().WithMany().HasForeignKey(a => a.HighestBidderUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(a => a.Bids)
                   .WithOne()
                   .HasForeignKey(b => b.AuctionId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(a => a.Bids).UsePropertyAccessMode(PropertyAccessMode.Field);

            // One open auction per listing at a time. The aggregate cannot see other auctions,
            // so this is the only place the rule is enforced.
            builder.HasIndex(a => a.VehicleListingId)
                   .IsUnique()
                   .HasFilter("status = 'Open'")
                   .HasDatabaseName("ix_auctions_one_open_per_listing");

            builder.HasIndex(a => new { a.Status, a.EndsAt });
        }
    }
}
