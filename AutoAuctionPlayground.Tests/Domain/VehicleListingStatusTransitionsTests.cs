using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class VehicleListingStatusTransitionsTests
    {
        [Theory]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Active, true)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Removed, true)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Sold, false)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Paused, false)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Active, VehicleListingStatus.Sold, true)]
        [InlineData(VehicleListingStatus.Active, VehicleListingStatus.Paused, true)]
        [InlineData(VehicleListingStatus.Active, VehicleListingStatus.Removed, true)]
        [InlineData(VehicleListingStatus.Active, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Active, VehicleListingStatus.Active, false)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Active, true)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Removed, true)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Sold, false)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Paused, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Removed, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Active, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Paused, false)]
        [InlineData(VehicleListingStatus.Removed, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Removed, VehicleListingStatus.Active, false)]
        [InlineData(VehicleListingStatus.Removed, VehicleListingStatus.Sold, false)]
        [InlineData(VehicleListingStatus.Removed, VehicleListingStatus.Paused, false)]
        public void CanTransitionTo_ReturnsExpectedResult(VehicleListingStatus current, VehicleListingStatus target, bool expected)
        {
            Assert.Equal(expected, current.CanTransitionTo(target));
        }

        [Theory]
        [InlineData(VehicleListingStatus.Draft)]
        [InlineData(VehicleListingStatus.Active)]
        [InlineData(VehicleListingStatus.Paused)]
        [InlineData(VehicleListingStatus.Sold)]
        [InlineData(VehicleListingStatus.Removed)]
        public void MessageFor_HasAMessageForEveryStatus(VehicleListingStatus target)
        {
            var message = VehicleListingStatusTransitions.MessageFor(target);

            Assert.False(string.IsNullOrWhiteSpace(message));
        }
    }
}
