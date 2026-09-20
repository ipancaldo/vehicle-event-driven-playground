using AutoAuctionPlayground.Application.Interfaces.Repositories;

namespace AutoAuctionPlayground.Application.Test.TestSupport
{
    internal sealed record FakeUnitOfWorkContext(
        IUnitOfWork UnitOfWork,
        ICompanyRepository Companies,
        IUserRepository Users,
        IVehicleMakeRepository VehicleMakes,
        IVehicleListingRepository VehicleListings,
        IAuctionRepository Auctions,
        IVehicleTransactionRepository VehicleTransactions);

    /// <summary>
    /// A ready-to-use IUnitOfWork substitute with every repository property pre-wired to its own
    /// substitute (unconfigured NSubstitute interface properties return null, which would NRE the
    /// moment a handler calls uow.Users.GetById(...)). StartTransaction is configured to actually
    /// invoke the given action against these same fakes, so handlers that use it (Sell) can be
    /// tested the same way as ones that call Commit directly (Create/Update/Publish).
    /// </summary>
    internal static class FakeUnitOfWork
    {
        public static FakeUnitOfWorkContext Create()
        {
            var uow = Substitute.For<IUnitOfWork>();

            var companies = Substitute.For<ICompanyRepository>();
            var users = Substitute.For<IUserRepository>();
            var vehicleMakes = Substitute.For<IVehicleMakeRepository>();
            var vehicleListings = Substitute.For<IVehicleListingRepository>();
            var auctions = Substitute.For<IAuctionRepository>();
            var vehicleTransactions = Substitute.For<IVehicleTransactionRepository>();

            uow.Companies.Returns(companies);
            uow.Users.Returns(users);
            uow.VehicleMakes.Returns(vehicleMakes);
            uow.VehicleListings.Returns(vehicleListings);
            uow.Auctions.Returns(auctions);
            uow.VehicleTransactions.Returns(vehicleTransactions);

            uow.StartTransaction(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>())
                .Returns(callInfo => callInfo.Arg<Func<Task>>()());

            return new FakeUnitOfWorkContext(uow, companies, users, vehicleMakes, vehicleListings, auctions, vehicleTransactions);
        }
    }
}
