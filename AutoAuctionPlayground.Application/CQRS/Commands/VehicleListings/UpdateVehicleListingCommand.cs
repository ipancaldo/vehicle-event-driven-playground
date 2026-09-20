using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Interfaces.Repositories;

namespace AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings
{
    public sealed record UpdateVehicleListingCommand(Guid Id, UpdateVehicleListingRequestDTO Dto) : ICommand;

    public sealed class UpdateVehicleListingCommandHandler(IUnitOfWork uow) : ICommandHandler<UpdateVehicleListingCommand>
    {
        public async Task Handle(UpdateVehicleListingCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;

            var listing = await uow.VehicleListings.GetAggregateById(command.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Vehicle listing '{command.Id}' not found.");

            if (dto.Price.HasValue)
                listing.UpdatePrice(dto.Price.Value, dto.UpdatedByUserId);

            if (dto.MileageKm.HasValue)
                listing.UpdateMileage(dto.MileageKm.Value);

            await uow.Commit(cancellationToken);
        }
    }
}
