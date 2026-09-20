using AutoAuctionPlayground.Domain.Entities.Vehicle;

namespace AutoAuctionPlayground.Application.Interfaces.Repositories
{
    public interface IVehicleListingRepository : IBaseRepository<VehicleListing>
    {
        // Loads the full aggregate (model + make + price history) for a write-side use case.
        Task<VehicleListing?> GetAggregateById(Guid id, CancellationToken cancellationToken = default);

        // Backs the "VIN unique per company among Draft/Published/Paused" rule; the database index
        // is the real guard, this is the friendly check that runs first.
        Task<bool> HasActiveListingWithVin(Guid dealerCompanyId, string vin, Guid? excludeListingId = null, CancellationToken cancellationToken = default);
    }
}
