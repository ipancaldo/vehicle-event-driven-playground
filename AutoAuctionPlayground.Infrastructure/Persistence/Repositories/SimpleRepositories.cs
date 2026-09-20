using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Transactions;
using AutoAuctionPlayground.Domain.Entities.Users;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Repositories
{
    public class CompanyRepository(AutoAuctionDbContext context)
        : BaseRepository<Company>(context), ICompanyRepository;

    public class UserRepository(AutoAuctionDbContext context)
        : BaseRepository<User>(context), IUserRepository;

    public class VehicleTransactionRepository(AutoAuctionDbContext context)
        : BaseRepository<VehicleTransaction>(context), IVehicleTransactionRepository;
}
