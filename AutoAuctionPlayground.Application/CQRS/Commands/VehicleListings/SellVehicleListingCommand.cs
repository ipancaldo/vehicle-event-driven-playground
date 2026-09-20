using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Domain.Entities.Transactions;

namespace AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings
{
    public sealed record SellVehicleListingCommand(Guid Id, SellVehicleListingDTO Dto) : ICommand;

    public sealed class SellVehicleListingCommandHandler(IUnitOfWork uow) : ICommandHandler<SellVehicleListingCommand>
    {
        public async Task Handle(SellVehicleListingCommand command, CancellationToken cancellationToken)
        {
            var listing = await uow.VehicleListings.GetById(command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Vehicle listing '{command.Id}' not found.");

            var buyer = await uow.Users.GetById(command.Dto.BuyerUserId, cancellationToken)
                ?? throw new KeyNotFoundException($"User '{command.Dto.BuyerUserId}' not found.");

            var now = DateTime.UtcNow;

            // The database's unique index on
            // vehicle_transactions.vehicle_listing_id is what stops two buyers racing each other —
            // a second concurrent Sell will fail Commit() with UniqueConstraintViolationException.
            await uow.StartTransaction(async () =>
            {
                var transaction = VehicleTransaction.CreateDirectSale(listing, buyer, now);
                await uow.VehicleTransactions.Add(transaction, cancellationToken);
                listing.MarkAsSold();
            }, cancellationToken);
        }
    }
}
