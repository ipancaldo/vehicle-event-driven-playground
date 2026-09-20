namespace AutoAuctionPlayground.Domain.Enums
{
    // Lite state machine
    public static class VehicleListingStatusTransitions
    {
        private static readonly Dictionary<VehicleListingStatus, VehicleListingStatus[]> _allowedTransitions = new()
        {
            [VehicleListingStatus.Draft] = [VehicleListingStatus.Published, VehicleListingStatus.Cancelled],
            [VehicleListingStatus.Published] = [VehicleListingStatus.Sold, VehicleListingStatus.Paused, VehicleListingStatus.Cancelled],
            [VehicleListingStatus.Paused] = [VehicleListingStatus.Published, VehicleListingStatus.Cancelled],
            [VehicleListingStatus.Sold] = [],
            [VehicleListingStatus.Cancelled] = []
        };
        public static bool CanTransitionTo(this VehicleListingStatus current, VehicleListingStatus target)
            => _allowedTransitions[current].Contains(target);

        private static readonly Dictionary<VehicleListingStatus, string> _transitionErrorMessages = new()
        {
            [VehicleListingStatus.Draft] = "Listings cannot be moved back to draft.",
            [VehicleListingStatus.Published] = "Only draft or paused listings can be published.",
            [VehicleListingStatus.Paused] = "Only published listings can be paused.",
            [VehicleListingStatus.Sold] = "Only published listings can be marked as sold.",
            [VehicleListingStatus.Cancelled] = "This listing cannot be cancelled from its current status."
        };
        public static string MessageFor(VehicleListingStatus target) => _transitionErrorMessages[target];
    }
}
