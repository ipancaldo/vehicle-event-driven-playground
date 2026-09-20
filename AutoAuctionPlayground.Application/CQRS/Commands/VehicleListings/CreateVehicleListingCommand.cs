using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Domain.Entities.Vehicle;

namespace AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings
{
    public sealed record CreateVehicleListingCommand(CreateVehicleListingRequestDTO Dto) : ICommand<Guid>;

    public sealed class CreateVehicleListingCommandHandler(IUnitOfWork uow) : ICommandHandler<CreateVehicleListingCommand, Guid>
    {
        public async Task<Guid> Handle(CreateVehicleListingCommand command, CancellationToken cancellationToken)
        {
            var dto = command.Dto;

            var dealer = await uow.Users.GetById(dto.DealerId, cancellationToken)
                ?? throw new KeyNotFoundException($"User '{dto.DealerId}' not found.");

            var vehicleModel = await uow.VehicleMakes.GetModelById(dto.VehicleModelId, cancellationToken)
                ?? throw new KeyNotFoundException($"Vehicle model '{dto.VehicleModelId}' not found.");

            // Friendly pre-check; the database's partial unique index is the real guard against a
            // race between two requests for the same VIN.
            if (await uow.VehicleListings.HasActiveListingWithVin(dealer.CompanyId, dto.Vin, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"An active listing with VIN '{dto.Vin}' already exists for this company.");

            var listing = VehicleListing.Create(dealer, dto.Vin, vehicleModel, dto.Year, dto.MileageKm, dto.Price);

            await uow.VehicleListings.Add(listing, cancellationToken);
            await uow.Commit(cancellationToken);

            return listing.Id;
        }
    }
}
