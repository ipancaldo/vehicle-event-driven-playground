using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Application.DTOs.VehicleListings
{
    public sealed record VehicleListingSummaryDTO(
        Guid Id,
        Guid DealerId,
        Guid DealerCompanyId,
        string Vin,
        string Make,
        string Model,
        int Year,
        int MileageKm,
        decimal Price,
        Currency Currency,
        VehicleListingStatus Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
