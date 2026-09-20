namespace AutoAuctionPlayground.Application.DTOs.VehicleListings
{
    public sealed record CreateVehicleListingRequestDTO(
        Guid DealerId,
        Guid VehicleModelId,
        string Vin,
        int Year,
        int MileageKm,
        decimal Price);
}
