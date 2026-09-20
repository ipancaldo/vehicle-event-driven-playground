using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Domain.Entities.Auctions;
using AutoAuctionPlayground.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Repositories
{
    public class AuctionRepository(AutoAuctionDbContext context)
        : BaseRepository<Auction>(context), IAuctionRepository
    {
        public Task<Auction?> GetWithBidsById(Guid id, CancellationToken cancellationToken = default)
            => _dbSet.Include(a => a.Bids).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        public Task<Auction?> GetOpenByListingId(Guid vehicleListingId, CancellationToken cancellationToken = default)
            => _dbSet.FirstOrDefaultAsync(
                a => a.VehicleListingId == vehicleListingId && a.Status == AuctionStatus.Open,
                cancellationToken);
    }
}
