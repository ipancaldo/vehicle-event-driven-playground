using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Seeders
{
    // Reference data: a small catalog of makes/models so listings have something real to point at.
    // Safe in every environment; no-ops once any make already exists.
    public static class VehicleMakeSeeder
    {
        private sealed record ModelSeed(string Name, int ProductionStartYear);
        private sealed record MakeSeed(string Name, ModelSeed[] Models);

        private static readonly MakeSeed[] Makes =
        [
            new("Ford", [new("Focus", 2010), new("Fiesta", 2008), new("Mondeo", 2007)]),
            new("Honda", [new("Civic", 2016), new("Accord", 2017), new("CR-V", 2016)]),
            new("Volkswagen", [new("Golf", 2012), new("Passat", 2014), new("Tiguan", 2015)]),
            new("Toyota", [new("Corolla", 2018), new("Camry", 2017), new("RAV4", 2018)]),
            new("BMW", [new("3 Series", 2018), new("5 Series", 2016), new("X5", 2018)]),
        ];

        public static async Task SeedAsync(AutoAuctionDbContext db, CancellationToken ct = default)
        {
            if (await db.VehicleMakes.AnyAsync(ct)) return;

            foreach (var makeSeed in Makes)
            {
                var make = VehicleMake.Create(makeSeed.Name);
                foreach (var modelSeed in makeSeed.Models)
                    make.AddModel(modelSeed.Name, modelSeed.ProductionStartYear);

                db.VehicleMakes.Add(make);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
