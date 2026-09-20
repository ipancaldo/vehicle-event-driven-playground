using AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Test.TestSupport;
using AutoAuctionPlayground.Domain.Entities.Companies;
using AutoAuctionPlayground.Domain.Entities.Users;
using AutoAuctionPlayground.Domain.Entities.Vehicle;

namespace AutoAuctionPlayground.Application.Test.CQRS.Commands.VehicleListings
{
    public class CreateVehicleListingCommandHandlerTests
    {
        private static User CreateDealer() => User.Create(Company.Create("Red & White"), "Alex Morgan");
        private static VehicleModel CreateModel() => VehicleMake.Create("Ford").AddModel("Focus", 1998);

        private static CreateVehicleListingRequestDTO ValidRequest(Guid dealerId, Guid modelId) =>
            new(dealerId, modelId, "ABC123", 2017, 50000, 15000m);

        [Fact]
        public async Task Handle_DealerNotFound_ThrowsKeyNotFoundException()
        {
            var fake = FakeUnitOfWork.Create();
            fake.Users.GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);
            var handler = new CreateVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new CreateVehicleListingCommand(ValidRequest(Guid.NewGuid(), Guid.NewGuid()));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_VehicleModelNotFound_ThrowsKeyNotFoundException()
        {
            var fake = FakeUnitOfWork.Create();
            var dealer = CreateDealer();
            fake.Users.GetById(dealer.Id, Arg.Any<CancellationToken>()).Returns(dealer);
            fake.VehicleMakes.GetModelById(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((VehicleModel?)null);
            var handler = new CreateVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new CreateVehicleListingCommand(ValidRequest(dealer.Id, Guid.NewGuid()));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ActiveListingAlreadyHasVin_ThrowsInvalidOperationException()
        {
            var fake = FakeUnitOfWork.Create();
            var dealer = CreateDealer();
            var model = CreateModel();
            fake.Users.GetById(dealer.Id, Arg.Any<CancellationToken>()).Returns(dealer);
            fake.VehicleMakes.GetModelById(model.Id, Arg.Any<CancellationToken>()).Returns(model);
            fake.VehicleListings
                .HasActiveListingWithVin(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(true);
            var handler = new CreateVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new CreateVehicleListingCommand(ValidRequest(dealer.Id, model.Id));

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidRequest_AddsListingAndCommits()
        {
            var fake = FakeUnitOfWork.Create();
            var dealer = CreateDealer();
            var model = CreateModel();
            fake.Users.GetById(dealer.Id, Arg.Any<CancellationToken>()).Returns(dealer);
            fake.VehicleMakes.GetModelById(model.Id, Arg.Any<CancellationToken>()).Returns(model);
            fake.VehicleListings
                .HasActiveListingWithVin(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(false);
            var handler = new CreateVehicleListingCommandHandler(fake.UnitOfWork);

            var command = new CreateVehicleListingCommand(ValidRequest(dealer.Id, model.Id));
            var id = await handler.Handle(command, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, id);
            await fake.VehicleListings.Received(1).Add(
                Arg.Is<VehicleListing>(listing => listing.Id == id && listing.DealerId == dealer.Id && listing.VehicleModelId == model.Id),
                Arg.Any<CancellationToken>());
            await fake.UnitOfWork.Received(1).Commit(Arg.Any<CancellationToken>());
        }
    }
}
