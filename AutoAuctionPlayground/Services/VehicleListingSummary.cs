namespace AutoAuctionPlayground.Web.Services
{
    /// <summary>
    /// The Web app's own view of the listings API response. Deliberately not shared
    /// with Domain/Application: this is an HTTP contract the UI consumes, not a domain type.
    /// </summary>
    public sealed record VehicleListingSummary(
        Guid Id,
        Guid DealerId,
        string Vin,
        string Make,
        string Model,
        int Year,
        int MileageKm,
        decimal Price,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
