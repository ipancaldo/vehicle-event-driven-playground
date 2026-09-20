using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Seeders
{
    // Development-only fixture companies/users. "Default" is meant for manual local testing (e.g.
    // as the dealer or bidder when trying a flow out yourself); the rest exist so cross-company
    // rules (no bidding/buying your own listing) have real counterparties to test against.
    public static class CompanyUserSeeder
    {
        public sealed record SeedUsers(User Default, User BlueMotors1, User BlueMotors2, User GreenAuto, User SilverLine);

        public static async Task<SeedUsers> SeedAsync(AutoAuctionDbContext db, CancellationToken ct = default)
        {
            var redAndWhite = await GetOrCreateCompany(db, "Red & White", ct);
            var blueMotors = await GetOrCreateCompany(db, "Blue Motors", ct);
            var greenAuto = await GetOrCreateCompany(db, "Green Auto Traders", ct);
            var silverLine = await GetOrCreateCompany(db, "Silver Line Dealers", ct);

            // Alex Morgan / Red & White is the default: use it as "yourself" when testing locally.
            var defaultUser = await GetOrCreateUser(db, redAndWhite, "Alex Morgan", ct);
            var blueMotors1 = await GetOrCreateUser(db, blueMotors, "Jamie Lee", ct);
            var blueMotors2 = await GetOrCreateUser(db, blueMotors, "Taylor Reed", ct);
            var greenAutoUser = await GetOrCreateUser(db, greenAuto, "Morgan Diaz", ct);
            var silverLineUser = await GetOrCreateUser(db, silverLine, "Casey Nguyen", ct);

            await db.SaveChangesAsync(ct);

            return new SeedUsers(defaultUser, blueMotors1, blueMotors2, greenAutoUser, silverLineUser);
        }

        private static async Task<Company> GetOrCreateCompany(AutoAuctionDbContext db, string name, CancellationToken ct)
        {
            var existing = await db.Companies.FirstOrDefaultAsync(c => c.Name == name, ct);
            if (existing is not null)
                return existing;

            var company = Company.Create(name);
            db.Companies.Add(company);
            return company;
        }

        private static async Task<User> GetOrCreateUser(AutoAuctionDbContext db, Company company, string name, CancellationToken ct)
        {
            var existing = await db.Users.FirstOrDefaultAsync(u => u.CompanyId == company.Id && u.Name == name, ct);
            if (existing is not null)
                return existing;

            var user = User.Create(company, name);
            db.Users.Add(user);
            return user;
        }
    }
}
