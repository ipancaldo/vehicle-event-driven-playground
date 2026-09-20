namespace AutoAuctionPlayground.Domain.Entities.Vehicle
{
    // Append-only. The current price is VehicleListing.Details.Price; this is only the trail of
    // how it got there, so there is deliberately no "IsCurrent" flag to keep in sync.
    public class ListingPriceChange
    {
        public Guid Id { get; private set; }
        public Guid VehicleListingId { get; private set; }
        public decimal OldPrice { get; private set; }
        public decimal NewPrice { get; private set; }
        public Guid ChangedByUserId { get; private set; }
        public DateTime ChangedAt { get; private set; }

        private ListingPriceChange() { }
        private ListingPriceChange(Guid vehicleListingId, decimal oldPrice, decimal newPrice, Guid changedByUserId)
        {
            Id = Guid.NewGuid();
            VehicleListingId = vehicleListingId;
            OldPrice = oldPrice;
            NewPrice = newPrice;
            ChangedByUserId = changedByUserId;
            ChangedAt = DateTime.UtcNow;
        }

        internal static ListingPriceChange Create(Guid vehicleListingId, decimal oldPrice, decimal newPrice, Guid changedByUserId)
            => new(vehicleListingId, oldPrice, newPrice, changedByUserId);
    }
}
