using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Interfaces.Queries;
using AutoAuctionPlayground.Domain.Enums;

namespace AutoAuctionPlayground.Application.CQRS.Queries.VehicleListings
{
    public sealed record GetVehicleListingsQuery(VehicleListingStatus? Status, Guid? DealerCompanyId)
        : IQuery<IReadOnlyList<VehicleListingSummaryDTO>>;

    public sealed class GetVehicleListingsQueryHandler(IVehicleListingQueries queries)
        : IQueryHandler<GetVehicleListingsQuery, IReadOnlyList<VehicleListingSummaryDTO>>
    {
        public Task<IReadOnlyList<VehicleListingSummaryDTO>> Handle(GetVehicleListingsQuery query, CancellationToken cancellationToken)
            => queries.List(query.Status, query.DealerCompanyId, cancellationToken);
    }
}
