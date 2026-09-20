namespace AutoAuctionPlayground.Domain.ValueObjects
{
    public sealed record VehicleListingDetails
    {
        private const int EarliestListingYear = 1900;

        public string Vin { get; private set; } = default!;
        public int Year { get; private set; }
        public int MileageKm { get; private set; }
        public decimal Price { get; private set; }

        private VehicleListingDetails() { }

        private VehicleListingDetails(
            string vin,
            int year,
            int mileageKm,
            decimal price)
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
            decimal price)
        {
            EnsureValidVin(vin);
            EnsureValidYear(year);
            EnsureValidMileage(mileageKm, nameof(mileageKm));
            EnsureValidPrice(price, nameof(price));

            return new VehicleListingDetails(vin, year, mileageKm, price);
        }

        public VehicleListingDetails UpdateMileage(int newMileageKm)
        {
            return Update(Vin, Year, newMileageKm, Price);
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
            var updated = Create(vin, year, mileageKm, price);

            if (updated.MileageKm < MileageKm)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(mileageKm),
                    "Mileage cannot decrease.");
            }

            return updated;
        }

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

        private static void EnsureValidPrice(decimal price, string parameterName)
        {
            if (price < 0)
                throw new ArgumentOutOfRangeException(parameterName, "Price cannot be negative.");
        }
    }
}
