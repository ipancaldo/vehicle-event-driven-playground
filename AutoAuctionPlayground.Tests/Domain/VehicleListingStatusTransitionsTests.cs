using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Tests.Domain
{
    public class VehicleListingStatusTransitionsTests
    {
        [Theory]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Published, true)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Cancelled, true)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Sold, false)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Paused, false)]
        [InlineData(VehicleListingStatus.Draft, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Published, VehicleListingStatus.Sold, true)]
        [InlineData(VehicleListingStatus.Published, VehicleListingStatus.Paused, true)]
        [InlineData(VehicleListingStatus.Published, VehicleListingStatus.Cancelled, true)]
        [InlineData(VehicleListingStatus.Published, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Published, VehicleListingStatus.Published, false)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Published, true)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Cancelled, true)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Sold, false)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Paused, VehicleListingStatus.Paused, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Cancelled, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Published, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Sold, VehicleListingStatus.Paused, false)]
        [InlineData(VehicleListingStatus.Cancelled, VehicleListingStatus.Draft, false)]
        [InlineData(VehicleListingStatus.Cancelled, VehicleListingStatus.Published, false)]
        [InlineData(VehicleListingStatus.Cancelled, VehicleListingStatus.Sold, false)]
        [InlineData(VehicleListingStatus.Cancelled, VehicleListingStatus.Paused, false)]
        public void CanTransitionTo_ReturnsExpectedResult(VehicleListingStatus current, VehicleListingStatus target, bool expected)
        {
            Assert.Equal(expected, current.CanTransitionTo(target));
        }

        [Theory]
        [InlineData(VehicleListingStatus.Draft)]
        [InlineData(VehicleListingStatus.Published)]
        [InlineData(VehicleListingStatus.Paused)]
        [InlineData(VehicleListingStatus.Sold)]
        [InlineData(VehicleListingStatus.Cancelled)]
        public void MessageFor_HasAMessageForEveryStatus(VehicleListingStatus target)
        {
            var message = VehicleListingStatusTransitions.MessageFor(target);

            Assert.False(string.IsNullOrWhiteSpace(message));
        }
    }
}
