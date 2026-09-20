namespace AutoAuctionPlayground.Application.DTOs.VehicleListings
{
    // The listing id comes from the route; this only needs who's buying. No payment concept
    // exists in the domain (payments are explicitly out of scope for this project).
    public sealed record SellVehicleListingDTO(Guid BuyerUserId);
}
