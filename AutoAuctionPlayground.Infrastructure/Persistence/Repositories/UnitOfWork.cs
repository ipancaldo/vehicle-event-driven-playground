using AutoAuctionPlayground.Application.Exceptions;
using AutoAuctionPlayground.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork(AutoAuctionDbContext context, IRepositoryFactory repositoryFactory) : IUnitOfWork
    {
        private const string PostgresUniqueViolation = "23505";

        private readonly AutoAuctionDbContext _context = context;
        private readonly IRepositoryFactory _repositoryFactory = repositoryFactory;

        public ICompanyRepository Companies => _repositoryFactory.GetRepository<ICompanyRepository>();
        public IUserRepository Users => _repositoryFactory.GetRepository<IUserRepository>();
        public IVehicleMakeRepository VehicleMakes => _repositoryFactory.GetRepository<IVehicleMakeRepository>();
        public IVehicleListingRepository VehicleListings => _repositoryFactory.GetRepository<IVehicleListingRepository>();
        public IAuctionRepository Auctions => _repositoryFactory.GetRepository<IAuctionRepository>();
        public IVehicleTransactionRepository VehicleTransactions => _repositoryFactory.GetRepository<IVehicleTransactionRepository>();

        public async Task StartTransaction(Func<Task> action, CancellationToken cancellationToken = default)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action();
                await Commit(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<int> Commit(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            // DbUpdateConcurrencyException (stale Version) is deliberately NOT caught here: the
            // command handler that owns the retry loop must see it.
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresUniqueViolation } pg)
            {
                throw new UniqueConstraintViolationException(pg.ConstraintName ?? "unknown");
            }
        }

        public void Dispose() => _context.Dispose();
    }
}
