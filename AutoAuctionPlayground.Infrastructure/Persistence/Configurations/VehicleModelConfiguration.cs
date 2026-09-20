using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
    {
        public void Configure(EntityTypeBuilder<VehicleModel> builder)
        {
            builder.ToTable("vehicle_models");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedNever(); // domain-assigned Guid, not store-generated

            builder.Property(m => m.Name).HasMaxLength(100).IsRequired();

            // Mirrors the in-memory "unique model name per make" rule in VehicleMake.AddModel.
            builder.HasIndex(m => new { m.VehicleMakeId, m.Name }).IsUnique();
        }
    }
}
