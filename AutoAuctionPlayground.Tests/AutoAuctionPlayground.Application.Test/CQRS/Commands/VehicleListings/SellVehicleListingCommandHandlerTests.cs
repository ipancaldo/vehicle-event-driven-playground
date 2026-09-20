using AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Test.TestSupport;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Transactions;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Application.Test.CQRS.Commands.VehicleListings
{
    public class SellVehicleListingCommandHandlerTests
    {
        private static VehicleListing CreatePublishedListing(out User dealer)
        {
            dealer = User.Create(Company.Create("Red & White"), "Alex Morgan");
            var model = VehicleMake.Create("Ford").AddModel("Focus", 1998);
            var listing = VehicleListing.Create(dealer, "ABC123", model, 2017, 50000, 15000m);
            listing.Publish();
            return listing;
        }

        [Fact]
        public async Task Handle_ListingNotFound_ThrowsKeyNotFoundException()
        {
            var fake = FakeUnitOfWork.Create();
            fake.VehicleListings.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((VehicleListing?)null);
            var handler = new SellVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new SellVehicleListingCommand(Guid.NewGuid(), new SellVehicleListingDTO(Guid.NewGuid()));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BuyerNotFound_ThrowsKeyNotFoundException()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreatePublishedListing(out _);
            fake.VehicleListings.GetById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            fake.Users.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);
            var handler = new SellVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new SellVehicleListingCommand(listing.Id, new SellVehicleListingDTO(Guid.NewGuid()));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidSale_UsesTransactionWrapperAndMarksListingSold()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreatePublishedListing(out _);
            var buyer = User.Create(Company.Create("Blue Motors"), "Jamie Lee");
            fake.VehicleListings.GetById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            fake.Users.GetById(buyer.Id, Arg.Any<CancellationToken>()).Returns(buyer);
            var handler = new SellVehicleListingCommandHandler(fake.UnitOfWork);

            await handler.Handle(new SellVehicleListingCommand(listing.Id, new SellVehicleListingDTO(buyer.Id)), CancellationToken.None);

            // The write spans two aggregates (the listing and the new transaction), so it must go
            // through StartTransaction rather than a plain Add+Commit — this is the one thing a
            // fake can verify that a real end-to-end test would only prove indirectly.
            await fake.UnitOfWork.Received(1).StartTransaction(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>());
            await fake.VehicleTransactions.Received(1).Add(
                Arg.Is<VehicleTransaction>(t => t.VehicleListingId == listing.Id && t.BuyerUserId == buyer.Id),
                Arg.Any<CancellationToken>());
            Assert.Equal(VehicleListingStatus.Sold, listing.Status);
        }

        [Fact]
        public async Task Handle_ListingNotPublished_PropagatesDomainRejection()
        {
            var fake = FakeUnitOfWork.Create();
            var dealer = User.Create(Company.Create("Red & White"), "Alex Morgan");
            var model = VehicleMake.Create("Ford").AddModel("Focus", 1998);
            var draftListing = VehicleListing.Create(dealer, "ABC123", model, 2017, 50000, 15000m); // never published
            var buyer = User.Create(Company.Create("Blue Motors"), "Jamie Lee");
            fake.VehicleListings.GetById(draftListing.Id, Arg.Any<CancellationToken>()).Returns(draftListing);
            fake.Users.GetById(buyer.Id, Arg.Any<CancellationToken>()).Returns(buyer);
            var handler = new SellVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new SellVehicleListingCommand(draftListing.Id, new SellVehicleListingDTO(buyer.Id));

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BuyerFromSellersOwnCompany_PropagatesDomainRejection()
        {
            var fake = FakeUnitOfWork.Create();
            var listing = CreatePublishedListing(out var dealer);
            var colleague = User.Create(dealer.Company, "Colleague");
            fake.VehicleListings.GetById(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
            fake.Users.GetById(colleague.Id, Arg.Any<CancellationToken>()).Returns(colleague);
            var handler = new SellVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new SellVehicleListingCommand(listing.Id, new SellVehicleListingDTO(colleague.Id));

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal(VehicleListingStatus.Published, listing.Status); // unchanged
        }
    }
}
