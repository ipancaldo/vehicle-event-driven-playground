using System.Text.RegularExpressions;

namespace AutoAuctionPlayground.Infrastructure.Extensions
{
    public static partial class NamingExtensions
    {
        // "MileageKm" -> "mileage_km", "PK_VehicleListings" -> "pk_vehicle_listings"
        public static string ToSnakeCase(this string value)
            => SnakeCaseBoundary().Replace(value, "_$0").ToLowerInvariant();

        [GeneratedRegex("(?<=[a-z0-9])[A-Z]|(?<=[A-Z])[A-Z](?=[a-z])")]
        private static partial Regex SnakeCaseBoundary();
    }
}
