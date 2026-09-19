using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Domain.Entities
{
    public class VehicleListing
    {
        public Guid Id { get; private set; }
        public Guid DealerId { get; private set; }
        public string VIN { get; private set; } = default!;
        public string Make { get; private set; } = default!;
        public string Model { get; private set; } = default!;
        public int Year { get; private set; }
        public int MileageKm { get; private set; }
        public decimal Price { get; private set; }
        public VehicleListingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private VehicleListing() { }
        private VehicleListing(Guid dealerId, string vin, string make, string model, int year, int mileage, decimal price)
        {
            Id = Guid.NewGuid();
            DealerId = dealerId;
            VIN = vin;
            Make = make;
            Model = model;
            Year = year;
            MileageKm = mileage;
            Price = price;
            Status = VehicleListingStatus.Draft;
            CreatedAt = DateTime.UtcNow;
        }

        public static VehicleListing Create(Guid dealerId, string vin, string make, string model, int year, int mileage, decimal price)
        {
            if (dealerId == Guid.Empty)
                throw new ArgumentException("DealerId is required.", nameof(dealerId));
            if (string.IsNullOrWhiteSpace(vin))
                throw new ArgumentException("VIN is required.", nameof(vin));
            if (string.IsNullOrWhiteSpace(make))
                throw new ArgumentException("Make is required.", nameof(make));
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model is required.", nameof(model));
            if (year < 1900 || year > DateTime.UtcNow.Year + 1)
                throw new ArgumentOutOfRangeException(nameof(year), "Year is not valid.");
            if (mileage < 0)
                throw new ArgumentOutOfRangeException(nameof(mileage), "Mileage cannot be negative.");
            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

            return new VehicleListing(dealerId, vin, make, model, year, mileage, price);
        }

        public void UpdatePrice(decimal newPrice)
        {
            EnsureNotRemoved();
            if (newPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative.");

            Price = newPrice;
            Touch();
        }

        public void UpdateMileage(int newMileage)
        {
            EnsureNotRemoved();
            if (newMileage < MileageKm)
                throw new ArgumentOutOfRangeException(nameof(newMileage), "Mileage cannot decrease.");

            MileageKm = newMileage;
            Touch();
        }

        public void Publish()
        {
            UpdateVehicleListingStatus(VehicleListingStatus.Active);
        }

        public void MarkAsSold()
        {
            UpdateVehicleListingStatus(VehicleListingStatus.Sold);
        }

        public void Remove()
        {
            UpdateVehicleListingStatus(VehicleListingStatus.Removed);
        }

        private void EnsureNotRemoved()
        {
            if (Status == VehicleListingStatus.Removed)
                throw new InvalidOperationException("Cannot modify a removed listing.");
        }

        private void UpdateVehicleListingStatus(VehicleListingStatus status)
        {
            EnsureNotRemoved();
            if (!Status.CanTransitionTo(status))
                throw new InvalidOperationException(VehicleListingStatusTransitions.MessageFor(status));

            Status = status;
            Touch();
        }

        private void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}
