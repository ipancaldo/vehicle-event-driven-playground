namespace AutoAuctionPlayground.Application.DTOs.VehicleListings
{
    // Only price and mileage are editable after creation — VIN/make/model/year are fixed at
    // creation, matching what VehicleListing itself exposes (UpdatePrice/UpdateMileage only).
    // Both fields are optional so a caller can update either one without resending the other;
    // UpdatedByUserId is only used when Price is set, to attribute the price-history entry.
    public sealed record UpdateVehicleListingRequestDTO(
        decimal? Price,
        int? MileageKm,
        Guid UpdatedByUserId);
}
