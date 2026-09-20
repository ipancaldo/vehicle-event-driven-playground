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
            // Id is assigned by the domain (Guid.NewGuid()), never by the database. Without this,
            // EF's default convention for Guid keys treats the non-default value as "must already
            // exist" whenever this entity is discovered by appending to an already-tracked parent's
            // collection (e.g. VehicleListing.UpdatePrice) rather than via an explicit Add() on a
            // brand-new graph — it then emits an UPDATE for a row that was never inserted, which
            // affects 0 rows and throws DbUpdateConcurrencyException.
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.ComplexProperty(p => p.OldPrice, price => price.ConfigureMoney("old_price"));
            builder.ComplexProperty(p => p.NewPrice, price => price.ConfigureMoney("new_price"));

            builder.HasIndex(p => new { p.VehicleListingId, p.ChangedAt });
        }
    }
}
