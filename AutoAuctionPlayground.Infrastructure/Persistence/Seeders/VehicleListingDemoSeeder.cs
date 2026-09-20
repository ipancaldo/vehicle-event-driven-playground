using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Entities.Transactions;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Seeders
{
    // Development-only fixture listings covering every lifecycle shape the API needs to exercise:
    // two plain published listings, one sold directly, one auction still open with no bids (ready
    // for a real PlaceBid call), and one closed auction with a simulated bidding war and a winner.
    // Depends on VehicleMakeSeeder and CompanyUserSeeder having already run.
    public static class VehicleListingDemoSeeder
    {
        // Fixed placeholder VINs (not real vehicles) so re-running the seeder is idempotent.
        private const string CivicVin = "SEEDHONDACIVIC001";
        private const string CorollaVin = "SEEDTOYOTACOROLLA";
        private const string FocusVin = "SEEDFORDFOCUS0003";
        private const string BmwVin = "SEEDBMW3SERIES004";
        private const string GolfVin = "SEEDVWGOLF0000005";

        private static readonly string[] SeedVins = [CivicVin, CorollaVin, FocusVin, BmwVin, GolfVin];

        public static async Task SeedAsync(AutoAuctionDbContext db, CompanyUserSeeder.SeedUsers users, CancellationToken ct = default)
        {
            if (await db.VehicleListings.AnyAsync(l => SeedVins.Contains(l.Details.Vin), ct))
                return;

            var civic = await GetModel(db, "Honda", "Civic", ct);
            var corolla = await GetModel(db, "Toyota", "Corolla", ct);
            var focus = await GetModel(db, "Ford", "Focus", ct);
            var series3 = await GetModel(db, "BMW", "3 Series", ct);
            var golf = await GetModel(db, "Volkswagen", "Golf", ct);

            var now = DateTime.UtcNow;

            // 1. Published, unsold — the default user's own listing.
            var civicListing = VehicleListing.Create(users.Default, CivicVin, civic, 2021, 15000, 21500m);
            civicListing.Publish();

            // 2. Published, unsold.
            var corollaListing = VehicleListing.Create(users.BlueMotors1, CorollaVin, corolla, 2019, 42000, 15800m);
            corollaListing.Publish();

            // 3. Published, then sold directly — the "1 normal also finished" listing.
            var focusListing = VehicleListing.Create(users.BlueMotors2, FocusVin, focus, 2017, 68000, 9800m);
            focusListing.Publish();
            var directSale = VehicleTransaction.CreateDirectSale(focusListing, users.GreenAuto, now.AddDays(-3));
            focusListing.MarkAsSold();

            // 4. Published, open auction, zero bids — left ready for a real PlaceBid call later.
            var bmwListing = VehicleListing.Create(users.GreenAuto, BmwVin, series3, 2022, 8000, 38000m);
            bmwListing.Publish();
            var openAuction = Auction.Create(bmwListing, startingPrice: 35000m, now.AddHours(-2), now.AddDays(5));

            // 5. Published, closed auction with a simulated three-way bidding war — the
            // "1 auction should be finished" listing. Each bidder is from a different company than
            // the seller (Silver Line Dealers), which PlaceBid requires.
            var golfListing = VehicleListing.Create(users.SilverLine, GolfVin, golf, 2020, 25000, 17500m);
            golfListing.Publish();

            var auctionCreatedAt = now.AddDays(-10);
            var auctionEndsAt = now.AddDays(-7);
            var closedAuction = Auction.Create(golfListing, startingPrice: 15000m, auctionCreatedAt, auctionEndsAt);
            closedAuction.PlaceBid(users.BlueMotors1, 15500m, auctionCreatedAt.AddHours(2));   // Jamie Lee
            closedAuction.PlaceBid(users.GreenAuto, 16200m, auctionCreatedAt.AddDays(1));       // Morgan Diaz outbids
            closedAuction.PlaceBid(users.Default, 17000m, auctionCreatedAt.AddDays(2));         // Alex Morgan wins
            closedAuction.Close(auctionEndsAt);
            var auctionSale = VehicleTransaction.CreateFromAuction(closedAuction, auctionEndsAt.AddMinutes(5));
            golfListing.MarkAsSold();

            db.VehicleListings.Add(civicListing);
            db.VehicleListings.Add(corollaListing);
            db.VehicleListings.Add(focusListing);
            db.VehicleListings.Add(bmwListing);
            db.VehicleListings.Add(golfListing);
            db.Auctions.Add(openAuction);
            db.Auctions.Add(closedAuction);
            db.VehicleTransactions.Add(directSale);
            db.VehicleTransactions.Add(auctionSale);

            await db.SaveChangesAsync(ct);
        }

        private static Task<VehicleModel> GetModel(AutoAuctionDbContext db, string makeName, string modelName, CancellationToken ct)
            => db.VehicleModels
                .Include(m => m.Make)
                .FirstAsync(m => m.Make.Name == makeName && m.Name == modelName, ct);
    }
}
