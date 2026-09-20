using AutoAuctionPlayground.Domain.ValueObjects;

namespace AutoAuctionPlayground.Domain.Entities.Vehicle
{
    // Append-only. The current price is VehicleListing.Details.Price; this is only the trail of
    // how it got there, so there is deliberately no "IsCurrent" flag to keep in sync.
    public class ListingPriceChange
    {
        public Guid Id { get; private set; }
        public Guid VehicleListingId { get; private set; }
        public Money OldPrice { get; private set; } = default!;
        public Money NewPrice { get; private set; } = default!;
        public Guid ChangedByUserId { get; private set; }
        public DateTime ChangedAt { get; private set; }

        private ListingPriceChange() { }
        private ListingPriceChange(Guid vehicleListingId, Money oldPrice, Money newPrice, Guid changedByUserId)
        {
            Id = Guid.NewGuid();
            VehicleListingId = vehicleListingId;
            OldPrice = oldPrice;
            NewPrice = newPrice;
            ChangedByUserId = changedByUserId;
            ChangedAt = DateTime.UtcNow;
        }

        internal static ListingPriceChange Create(Guid vehicleListingId, Money oldPrice, Money newPrice, Guid changedByUserId)
            => new(vehicleListingId, oldPrice, newPrice, changedByUserId);
    }
}
