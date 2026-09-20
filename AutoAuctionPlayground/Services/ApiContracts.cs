namespace AutoAuctionPlayground.Web.Services
{
    // The Web app's own view of the API's contracts. Deliberately not shared with
    // Domain/Application — these are HTTP shapes the UI consumes.

    public sealed record VehicleMakeSummary(Guid Id, string Name, IReadOnlyList<VehicleModelSummary> Models);

    public sealed record VehicleModelSummary(Guid Id, string Name, int ProductionStartYear, int? ProductionEndYear);

    public sealed record UserSummary(Guid Id, string Name, Guid CompanyId, string CompanyName)
    {
        public string Display => $"{Name} — {CompanyName}";
    }

    public sealed record CreateVehicleListingRequest(
        Guid DealerId,
        Guid VehicleModelId,
        string Vin,
        int Year,
        int MileageKm,
        decimal Price);

    public sealed record UpdateVehicleListingRequest(decimal? Price, int? MileageKm, Guid UpdatedByUserId);

    public sealed record SellVehicleListingRequest(Guid BuyerUserId);

    public sealed record CreatedListingResponse(Guid Id);

    // Failures are data, not exceptions: this console exists largely to trigger rejections
    // (duplicate VIN, double publish, double sale) and show what came back.
    public sealed record ApiResult(bool Success, int StatusCode, string? Error)
    {
        public static ApiResult Ok(int statusCode) => new(true, statusCode, null);
        public static ApiResult Fail(int statusCode, string? error) => new(false, statusCode, error);
    }

    public sealed record ApiResult<T>(bool Success, int StatusCode, T? Value, string? Error)
    {
        public static ApiResult<T> Ok(int statusCode, T value) => new(true, statusCode, value, null);
        public static ApiResult<T> Fail(int statusCode, string? error) => new(false, statusCode, default, error);
    }
}
