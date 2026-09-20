using AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings;
using AutoAuctionPlayground.Application.Test.TestSupport;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Application.Test.CQRS.Commands.VehicleListings
{
    public class PublishVehicleListingCommandHandlerTests
    {
        private static VehicleListing CreateDraftListing()
        {
            var dealer = User.Create(Company.Create("Red & White"), "Alex Morgan");
            var model = VehicleMake.Create("Ford").AddModel("Focus", 1998);
            return VehicleListing.Create(dealer, "ABC123", model, 2017, 50000, 15000m);
        }

        [Fact]
        public async Task Handle_ListingNotFound_ThrowsKeyNotFoundException()
        {
            var fake = FakeUnitOfWork.Create();
            fake.VehicleListings.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((VehicleListing?)null);
            var handler = new PublishVehicleListingCommandHandler(fake.UnitOfWork);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => handler.Handle(new PublishVehicleListingCommand(Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DraftListing_PublishesAndCommits()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreateDraftListing();
            fake.VehicleListings.GetById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            var handler = new PublishVehicleListingCommandHandler(fake.UnitOfWork);

            await handler.Handle(new PublishVehicleListingCommand(listing.Id), CancellationToken.None);

            Assert.Equal(VehicleListingStatus.Published, listing.Status);
            await fake.UnitOfWork.Received(1).Commit(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_AlreadyPublishedListing_PropagatesDomainRejection()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreateDraftListing();
            listing.Publish();
            fake.VehicleListings.GetById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            var handler = new PublishVehicleListingCommandHandler(fake.UnitOfWork);

            // The handler must not swallow or reinterpret the domain's own rejection.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(new PublishVehicleListingCommand(listing.Id), CancellationToken.None));
        }
    }
}
