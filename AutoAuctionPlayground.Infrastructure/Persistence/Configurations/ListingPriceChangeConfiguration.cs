using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class ListingPriceChangeConfiguration : IEntityTypeConfiguration<ListingPriceChange>
    {
        public void Configure(EntityTypeBuilder<ListingPriceChange> builder)
        {
            builder.ToTable("listing_price_changes");
            builder.HasKey(p => p.Id);

            builder.ComplexProperty(p => p.OldPrice, price => price.ConfigureMoney("old_price"));
            builder.ComplexProperty(p => p.NewPrice, price => price.ConfigureMoney("new_price"));

            builder.HasIndex(p => new { p.VehicleListingId, p.ChangedAt });
        }
    }
}
