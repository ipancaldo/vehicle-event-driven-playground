using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.DTOs.VehicleMakes;
using AutoAuctionPlayground.Application.Interfaces.Queries;

namespace AutoAuctionPlayground.Application.CQRS.Queries.VehicleMakes
{
    public sealed record GetVehicleMakesQuery : IQuery<IReadOnlyList<VehicleMakeDTO>>;

    public sealed class GetVehicleMakesQueryHandler(IVehicleMakeQueries queries)
        : IQueryHandler<GetVehicleMakesQuery, IReadOnlyList<VehicleMakeDTO>>
    {
        public Task<IReadOnlyList<VehicleMakeDTO>> Handle(GetVehicleMakesQuery query, CancellationToken cancellationToken)
            => queries.ListActive(cancellationToken);
    }
}
