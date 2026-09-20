using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class VehicleListingConfiguration : IEntityTypeConfiguration<VehicleListing>
    {
        public void Configure(EntityTypeBuilder<VehicleListing> builder)
        {
            builder.ToTable("vehicle_listings");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).ValueGeneratedNever(); // domain-assigned Guid, not store-generated

            // Details and its nested Price are pure value objects (no identity), mapped as complex
            // types rather than OwnsOne. See MoneyConfiguration for why: this listing also has a
            // PriceHistory collection whose entries own their own Money, and OwnsOne'ing Money in
            // both places on the same aggregate hit an EF Core bug.
            builder.ComplexProperty(l => l.Details, details =>
            {
                details.Property(d => d.Vin).HasColumnName("vin").HasMaxLength(17).IsRequired();
                details.Property(d => d.Year).HasColumnName("year");
                details.Property(d => d.MileageKm).HasColumnName("mileage_km");
                details.ComplexProperty(d => d.Price, price => price.ConfigureMoney("price"));
            });

            builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);

            // Cross-aggregate references are FKs only; the domain reaches other aggregates by id.
            builder.HasOne<User>().WithMany().HasForeignKey(l => l.DealerId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Company>().WithMany().HasForeignKey(l => l.DealerCompanyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Model)
                   .WithMany()
                   .HasForeignKey(l => l.VehicleModelId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(l => l.PriceHistory)
                   .WithOne()
                   .HasForeignKey(p => p.VehicleListingId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(l => l.PriceHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasIndex(l => l.Status);

            // "VIN unique per company among Draft/Published/Paused" is a partial unique index on
            // (dealer_company_id, vin). EF cannot declare a composite index that mixes an owner
            // column with an owned-type column, so it is created with raw SQL in the InitialCreate
            // migration. It is the real guard; IVehicleListingRepository.HasActiveListingWithVin is
            // only the friendly pre-check.
        }
    }
}
