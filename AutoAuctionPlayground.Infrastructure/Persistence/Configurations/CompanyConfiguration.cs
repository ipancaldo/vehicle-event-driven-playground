using AutoAuctionPlayground.Domain.Entities.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("companies");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
            builder.HasIndex(c => c.Name).IsUnique();

            builder.HasMany(c => c.Users)
                   .WithOne(u => u.Company)
                   .HasForeignKey(u => u.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.Navigation(c => c.Users).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
