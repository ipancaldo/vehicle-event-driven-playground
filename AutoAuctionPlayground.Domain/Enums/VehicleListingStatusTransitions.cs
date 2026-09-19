namespace AutoAuctionPlayground.Domain.Enums
{
    // Lite state machine
    public static class VehicleListingStatusTransitions
    {
        private static readonly Dictionary<VehicleListingStatus, VehicleListingStatus[]> _allowedTransitions = new()
        {
            [VehicleListingStatus.Draft] = [VehicleListingStatus.Active, VehicleListingStatus.Removed],
            [VehicleListingStatus.Active] = [VehicleListingStatus.Sold, VehicleListingStatus.Paused, VehicleListingStatus.Removed],
            [VehicleListingStatus.Paused] = [VehicleListingStatus.Active, VehicleListingStatus.Removed],
            [VehicleListingStatus.Sold] = [],
            [VehicleListingStatus.Removed] = []
        };
        public static bool CanTransitionTo(this VehicleListingStatus current, VehicleListingStatus target)
            => _allowedTransitions[current].Contains(target);

        private static readonly Dictionary<VehicleListingStatus, string> _transitionErrorMessages = new()
        {
            [VehicleListingStatus.Draft] = "Listings cannot be moved back to draft.",
            [VehicleListingStatus.Active] = "Only draft or paused listings can be published.",
            [VehicleListingStatus.Paused] = "Only active listings can be paused.",
            [VehicleListingStatus.Sold] = "Only active listings can be marked as sold.",
            [VehicleListingStatus.Removed] = "This listing cannot be removed from its current status."
        };
        public static string MessageFor(VehicleListingStatus target) => _transitionErrorMessages[target];
    }
}
