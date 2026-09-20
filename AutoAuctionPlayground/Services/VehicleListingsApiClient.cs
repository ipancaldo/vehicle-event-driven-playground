namespace AutoAuctionPlayground.Web.Services
{
    public class VehicleListingsApiClient(HttpClient httpClient, ApiActivityLog activityLog)
        : ApiClientBase(httpClient, activityLog)
    {
        private const string BasePath = "api/vehicle-listings";

        public async Task<IReadOnlyList<VehicleListingSummary>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await SendForResult<List<VehicleListingSummary>>(
                HttpMethod.Get, BasePath, cancellationToken: cancellationToken);

            return result.Success ? result.Value ?? [] : throw new HttpRequestException(result.Error);
        }

        public Task<ApiResult<CreatedListingResponse>> CreateAsync(
            CreateVehicleListingRequest request, string? note = null, CancellationToken cancellationToken = default)
            => SendForResult<CreatedListingResponse>(HttpMethod.Post, BasePath, request, note, cancellationToken);

        public Task<ApiResult> UpdateAsync(
            Guid id, UpdateVehicleListingRequest request, string? note = null, CancellationToken cancellationToken = default)
            => Send(HttpMethod.Put, $"{BasePath}/{id}", request, note, cancellationToken);

        public Task<ApiResult> PublishAsync(Guid id, string? note = null, CancellationToken cancellationToken = default)
            => Send(HttpMethod.Post, $"{BasePath}/{id}/publish", note: note, cancellationToken: cancellationToken);

        public Task<ApiResult> SellAsync(
            Guid id, Guid buyerUserId, string? note = null, CancellationToken cancellationToken = default)
            => Send(HttpMethod.Post, $"{BasePath}/{id}/sell", new SellVehicleListingRequest(buyerUserId), note, cancellationToken);
    }
}
