namespace AutoAuctionPlayground.Web.Services
{
    public class VehicleListingsApiClient
    {
        private readonly HttpClient _httpClient;

        public VehicleListingsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<VehicleListingSummary>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<VehicleListingSummary>>("api/vehicle-listings", cancellationToken);
            return result ?? [];
        }
    }
}
