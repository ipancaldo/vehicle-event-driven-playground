namespace AutoAuctionPlayground.Web.Services
{
    // Reference-data lookups the console needs: which makes/models can be listed, and which users
    // exist to act as dealer or buyer (there is no authentication yet, so the caller picks).
    public class CatalogueApiClient(HttpClient httpClient, ApiActivityLog activityLog)
        : ApiClientBase(httpClient, activityLog)
    {
        public async Task<IReadOnlyList<VehicleMakeSummary>> GetMakesAsync(CancellationToken cancellationToken = default)
        {
            var result = await SendForResult<List<VehicleMakeSummary>>(
                HttpMethod.Get, "api/vehicle-makes", cancellationToken: cancellationToken);

            return result.Success ? result.Value ?? [] : throw new HttpRequestException(result.Error);
        }

        public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var result = await SendForResult<List<UserSummary>>(
                HttpMethod.Get, "api/users", cancellationToken: cancellationToken);

            return result.Success ? result.Value ?? [] : throw new HttpRequestException(result.Error);
        }
    }
}
