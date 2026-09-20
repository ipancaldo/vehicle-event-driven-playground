using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Application.Interfaces.Queries
{
    // Read side. Implementations project straight from the database into DTOs (no tracking, no
    // aggregate loading), so listing screens never pay for Include chains they don't need.
    public interface IVehicleListingQueries
    {
        Task<IReadOnlyList<VehicleListingSummaryDto>> List(
            VehicleListingStatus? status = null,
            Guid? dealerCompanyId = null,
            CancellationToken cancellationToken = default);

        Task<VehicleListingSummaryDto?> GetById(Guid id, CancellationToken cancellationToken = default);
    }
}
