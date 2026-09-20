using AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Test.TestSupport;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;

namespace AutoAuctionPlayground.Application.Test.CQRS.Commands.VehicleListings
{
    public class UpdateVehicleListingCommandHandlerTests
    {
        // Real domain objects, not fakes: the aggregate's own invariants (mileage can only
        // increase, price history is appended) are already covered by the Domain tests. What this
        // handler needs verified is that it calls the real behavior instead of, say, setting the
        // fields directly — so the assertions below check the aggregate's resulting state.
        private static VehicleListing CreateListing()
        {
            var dealer = User.Create(Company.Create("Red & White"), "Alex Morgan");
            var model = VehicleMake.Create("Ford").AddModel("Focus", 1998);
            return VehicleListing.Create(dealer, "ABC123", model, 2017, 50000, 15000m);
        }

        [Fact]
        public async Task Handle_ListingNotFound_ThrowsKeyNotFoundException()
        {
            var fake = FakeUnitOfWork.Create();
            fake.VehicleListings.GetAggregateById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((VehicleListing?)null);
            var handler = new UpdateVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new UpdateVehicleListingCommand(Guid.NewGuid(), new UpdateVehicleListingRequestDTO(20000m, null, Guid.NewGuid()));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PriceProvided_UpdatesPriceOnly()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreateListing();
            fake.VehicleListings.GetAggregateById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            var handler = new UpdateVehicleListingCommandHandler(fake.UnitOfWork);
            var changedBy = Guid.NewGuid();

            await handler.Handle(new UpdateVehicleListingCommand(listing.Id, new UpdateVehicleListingRequestDTO(20000m, null, changedBy)), CancellationToken.None);

            Assert.Equal(20000m, listing.Details.Price.Amount);
            Assert.Equal(50000, listing.Details.MileageKm);
            Assert.Equal(changedBy, Assert.Single(listing.PriceHistory).ChangedByUserId);
            await fake.UnitOfWork.Received(1).Commit(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_MileageProvided_UpdatesMileageOnly()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreateListing();
            fake.VehicleListings.GetAggregateById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            var handler = new UpdateVehicleListingCommandHandler(fake.UnitOfWork);

            await handler.Handle(new UpdateVehicleListingCommand(listing.Id, new UpdateVehicleListingRequestDTO(null, 60000, Guid.NewGuid())), CancellationToken.None);

            Assert.Equal(60000, listing.Details.MileageKm);
            Assert.Equal(15000m, listing.Details.Price.Amount);
            Assert.Empty(listing.PriceHistory);
        }

        [Fact]
        public async Task Handle_NeitherFieldProvided_ChangesNothingButStillCommits()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreateListing();
            fake.VehicleListings.GetAggregateById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            var handler = new UpdateVehicleListingCommandHandler(fake.UnitOfWork);

            await handler.Handle(new UpdateVehicleListingCommand(listing.Id, new UpdateVehicleListingRequestDTO(null, null, Guid.NewGuid())), CancellationToken.None);

            Assert.Equal(15000m, listing.Details.Price.Amount);
            Assert.Equal(50000, listing.Details.MileageKm);
            Assert.Null(listing.UpdatedAt);
            await fake.UnitOfWork.Received(1).Commit(Arg.Any<CancellationToken>());
        }
    }
}
