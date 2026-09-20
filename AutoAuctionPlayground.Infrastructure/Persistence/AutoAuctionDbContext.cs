using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Transactions;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence
{
    public class AutoAuctionDbContext(DbContextOptions<AutoAuctionDbContext> options) : DbContext(options)
    {
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<User> Users => Set<User>();
        public DbSet<VehicleMake> VehicleMakes => Set<VehicleMake>();
        public DbSet<VehicleModel> VehicleModels => Set<VehicleModel>();
        public DbSet<VehicleListing> VehicleListings => Set<VehicleListing>();
        public DbSet<Auction> Auctions => Set<Auction>();
        public DbSet<VehicleTransaction> VehicleTransactions => Set<VehicleTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoAuctionDbContext).Assembly);
            modelBuilder.UseSnakeCaseNaming();
            base.OnModelCreating(modelBuilder);
        }
    }
}
