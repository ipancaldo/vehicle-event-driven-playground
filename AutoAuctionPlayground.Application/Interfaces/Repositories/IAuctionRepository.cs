using AutoAuctionPlayground.Domain.Entities.Auctions;

namespace AutoAuctionPlayground.Application.Interfaces.Repositories
{
    public interface IAuctionRepository : IBaseRepository<Auction>
    {
        Task<Auction?> GetWithBidsById(Guid id, CancellationToken cancellationToken = default);
        Task<Auction?> GetOpenByListingId(Guid vehicleListingId, CancellationToken cancellationToken = default);
    }
}
