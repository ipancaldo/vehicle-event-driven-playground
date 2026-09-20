using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Domain.ValueObjects
{
    public sealed record VehicleListingDetails
    {
        private const int EarliestListingYear = 1900;

        // Normalized (trimmed, upper-case) so the database uniqueness index on it means what it says.
        public string Vin { get; private set; } = default!;
        public int Year { get; private set; }
        public int MileageKm { get; private set; }
        public Money Price { get; private set; } = default!;

        private VehicleListingDetails() { }

        private VehicleListingDetails(
            string vin,
            int year,
            int mileageKm,
            Money price)
        {
            Vin = vin;
            Year = year;
            MileageKm = mileageKm;
            Price = price;
        }

        public static VehicleListingDetails Create(
            string vin,
            int year,
            int mileageKm,
            decimal price,
            Currency currency = Money.DefaultCurrency)
        {
            EnsureValidVin(vin);
            EnsureValidYear(year);
            EnsureValidMileage(mileageKm, nameof(mileageKm));

            return new VehicleListingDetails(NormalizeVin(vin), year, mileageKm, Money.Of(price, currency));
        }

        public VehicleListingDetails UpdateMileage(int newMileageKm)
        {
            return Update(Vin, Year, newMileageKm, Price.Amount);
        }

        public VehicleListingDetails UpdatePrice(decimal newPrice)
        {
            return Update(Vin, Year, MileageKm, newPrice);
        }

        public VehicleListingDetails Update(
            string vin,
            int year,
            int mileageKm,
            decimal price)
        {
            var updated = Create(vin, year, mileageKm, price, Price.Currency);

            if (updated.MileageKm < MileageKm)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(mileageKm),
                    "Mileage cannot decrease.");
            }

            return updated;
        }

        private static string NormalizeVin(string vin) => vin.Trim().ToUpperInvariant();

        private static void EnsureValidVin(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
                throw new ArgumentException("VIN is required.", nameof(vin));
        }

        private static void EnsureValidYear(int year)
        {
            if (year < EarliestListingYear || year > DateTime.UtcNow.Year + 1)
                throw new ArgumentOutOfRangeException(nameof(year), "Year is not valid.");
        }

        private static void EnsureValidMileage(int mileageKm, string parameterName)
        {
            if (mileageKm < 0)
                throw new ArgumentOutOfRangeException(parameterName, "Mileage cannot be negative.");
        }
    }
}
