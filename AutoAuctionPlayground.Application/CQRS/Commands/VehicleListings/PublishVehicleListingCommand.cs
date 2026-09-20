using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.Interfaces.Repositories;

namespace AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings
{
    public sealed record PublishVehicleListingCommand(Guid Id) : ICommand;

    public sealed class PublishVehicleListingCommandHandler(IUnitOfWork uow) : ICommandHandler<PublishVehicleListingCommand>
    {
        public async Task Handle(PublishVehicleListingCommand command, CancellationToken cancellationToken)
        {
            var listing = await uow.VehicleListings.GetById(command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Vehicle listing '{command.Id}' not found.");

            listing.Publish();

            await uow.Commit(cancellationToken);
        }
    }
}
