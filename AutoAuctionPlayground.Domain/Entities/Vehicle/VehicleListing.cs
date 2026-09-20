using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Enums;
using AutoAuctionPlayground.Domain.ValueObjects;

namespace AutoAuctionPlayground.Domain.Entities.Vehicle
{
    public class VehicleListing
    {
        private readonly List<ListingPriceChange> _priceHistory = [];

        public Guid Id { get; private set; }

        // The dealer is the User who listed the vehicle; DealerCompanyId is a snapshot of that
        // user's company at listing time so seller-side rules (no bidding or buying from your own
        // company) can be checked without loading the User aggregate.
        public Guid DealerId { get; private set; }
        public Guid DealerCompanyId { get; private set; }

        public Guid VehicleModelId { get; private set; }
        public VehicleModel Model { get; private set; } = default!;
        public VehicleListingDetails Details { get; private set; } = default!;
        public VehicleListingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public IReadOnlyCollection<ListingPriceChange> PriceHistory => _priceHistory.AsReadOnly();

        private VehicleListing() { }
        private VehicleListing(
            User dealer,
            VehicleModel model,
            VehicleListingDetails details)
        {
            Id = Guid.NewGuid();
            DealerId = dealer.Id;
            DealerCompanyId = dealer.CompanyId;
            VehicleModelId = model.Id;
            Model = model;
            Details = details;
            Status = VehicleListingStatus.Draft;
            CreatedAt = DateTime.UtcNow;
        }

        public static VehicleListing Create(
            User dealer,
            string vin,
            VehicleModel vehicleModel,
            int year,
            int mileage,
            decimal price)
        {
            ArgumentNullException.ThrowIfNull(dealer);

            var details = VehicleListingDetails.Create(vin, year, mileage, price);

            ArgumentNullException.ThrowIfNull(vehicleModel);
            vehicleModel.EnsureSelectableFor(details.Year);

            return new VehicleListing(dealer, vehicleModel, details);
        }

        // Concurrency hint: two colleagues editing the same listing is the "lost update" case.
        // This aggregate cannot detect it; the HTTP layer should require the version the client
        // loaded (ETag / If-Match) and the persistence layer a concurrency token, so the second
        // stale write is rejected instead of silently overwriting the first.
        public void UpdatePrice(decimal newPrice, Guid changedByUserId)
        {
            EnsureNotCancelled();
            var oldPrice = Details.Price;
            Details = Details.UpdatePrice(newPrice);
            _priceHistory.Add(ListingPriceChange.Create(Id, oldPrice, newPrice, changedByUserId));
            Touch();
        }

        public void UpdateMileage(int newMileage)
        {
            EnsureNotCancelled();
            Details = Details.UpdateMileage(newMileage);
            Touch();
        }

        public void Publish() => UpdateVehicleListingStatus(VehicleListingStatus.Published);

        public void Pause() => UpdateVehicleListingStatus(VehicleListingStatus.Paused);

        // Both Publish and Resume target Published, so the transition table alone can't tell them
        // apart: Resume additionally requires that the listing was actually paused.
        public void Resume()
        {
            EnsurePaused();
            UpdateVehicleListingStatus(VehicleListingStatus.Published);
        }

        public void MarkAsSold() => UpdateVehicleListingStatus(VehicleListingStatus.Sold);

        public void Cancel() => UpdateVehicleListingStatus(VehicleListingStatus.Cancelled);

        // Shared precondition for anything that sells the listing (auction, direct sale).
        public void EnsurePublished()
        {
            if (Status != VehicleListingStatus.Published)
                throw new InvalidOperationException($"Only published listings can be sold; this one is {Status}.");
        }

        private void EnsureNotCancelled()
        {
            if (Status == VehicleListingStatus.Cancelled)
                throw new InvalidOperationException("Cannot modify a cancelled listing.");
        }

        private void EnsurePaused()
        {
            if (Status != VehicleListingStatus.Paused)
                throw new InvalidOperationException("Only paused listings can be resumed.");
        }

        private void UpdateVehicleListingStatus(VehicleListingStatus status)
        {
            EnsureNotCancelled();
            if (!Status.CanTransitionTo(status))
                throw new InvalidOperationException(VehicleListingStatusTransitions.MessageFor(status));

            Status = status;
            Touch();
        }

        private void Touch() => UpdatedAt = DateTime.UtcNow;
    }
}
