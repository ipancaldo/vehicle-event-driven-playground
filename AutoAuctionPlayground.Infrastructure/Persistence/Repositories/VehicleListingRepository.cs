using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Repositories
{
    public class VehicleListingRepository(AutoAuctionDbContext context)
        : BaseRepository<VehicleListing>(context), IVehicleListingRepository
    {
        private static readonly VehicleListingStatus[] ActiveStatuses =
            [VehicleListingStatus.Draft, VehicleListingStatus.Published, VehicleListingStatus.Paused];

        public Task<VehicleListing?> GetAggregateById(Guid id, CancellationToken cancellationToken = default)
            => _dbSet
                .Include(l => l.Model).ThenInclude(m => m.Make)
                .Include(l => l.PriceHistory)
                .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        public Task<bool> HasActiveListingWithVin(Guid dealerCompanyId, string vin, Guid? excludeListingId = null, CancellationToken cancellationToken = default)
        {
            var normalizedVin = vin.Trim().ToUpperInvariant();
            var query = _dbSet.Where(l =>
                l.DealerCompanyId == dealerCompanyId &&
                l.Details.Vin == normalizedVin &&
                ActiveStatuses.Contains(l.Status));

            if (excludeListingId.HasValue)
                query = query.Where(l => l.Id != excludeListingId.Value);

            return query.AnyAsync(cancellationToken);
        }
    }
}
