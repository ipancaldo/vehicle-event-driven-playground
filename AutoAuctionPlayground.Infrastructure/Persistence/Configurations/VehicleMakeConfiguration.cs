using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class VehicleMakeConfiguration : IEntityTypeConfiguration<VehicleMake>
    {
        public void Configure(EntityTypeBuilder<VehicleMake> builder)
        {
            builder.ToTable("vehicle_makes");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedNever(); // domain-assigned Guid, not store-generated

            builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
            builder.HasIndex(m => m.Name).IsUnique();

            builder.HasMany(m => m.Models)
                   .WithOne(model => model.Make)
                   .HasForeignKey(model => model.VehicleMakeId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.Navigation(m => m.Models).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
