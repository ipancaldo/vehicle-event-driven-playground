using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Interfaces.Queries;

namespace AutoAuctionPlayground.Application.CQRS.Queries.VehicleListings
{
    public sealed record GetVehicleListingQuery(Guid Id) : IQuery<VehicleListingSummaryDto?>;

    public sealed class GetVehicleListingQueryHandler(IVehicleListingQueries queries)
        : IQueryHandler<GetVehicleListingQuery, VehicleListingSummaryDto?>
    {
        public Task<VehicleListingSummaryDto?> Handle(GetVehicleListingQuery query, CancellationToken cancellationToken)
            => queries.GetById(query.Id, cancellationToken);
    }
}
