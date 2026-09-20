namespace AutoAuctionPlayground.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICompanyRepository Companies { get; }
        IUserRepository Users { get; }
        IVehicleMakeRepository VehicleMakes { get; }
        IVehicleListingRepository VehicleListings { get; }
        IAuctionRepository Auctions { get; }
        IVehicleTransactionRepository VehicleTransactions { get; }

        // Runs the action inside one database transaction and commits it; anything the action
        // changes through the repositories above is written atomically or not at all.
        Task StartTransaction(Func<Task> action, CancellationToken cancellationToken = default);
        Task<int> Commit(CancellationToken cancellationToken = default);
    }
}
