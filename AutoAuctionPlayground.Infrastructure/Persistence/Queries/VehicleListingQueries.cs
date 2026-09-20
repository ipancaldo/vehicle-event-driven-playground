using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Application.Interfaces.Queries;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using AutoAuctionPlayground.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Queries
{
    // Read side: no change tracking, no aggregate materialisation. The projection joins model and
    // make in SQL and returns only the columns the DTO needs.
    public class VehicleListingQueries(AutoAuctionDbContext context) : IVehicleListingQueries
    {
        public async Task<IReadOnlyList<VehicleListingSummaryDTO>> List(
            VehicleListingStatus? status = null,
            Guid? dealerCompanyId = null,
            CancellationToken cancellationToken = default)
        {
            var query = context.VehicleListings.AsNoTracking();

            if (status.HasValue)
                query = query.Where(l => l.Status == status.Value);
            if (dealerCompanyId.HasValue)
                query = query.Where(l => l.DealerCompanyId == dealerCompanyId.Value);

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(Projection)
                .ToListAsync(cancellationToken);
        }

        public Task<VehicleListingSummaryDTO?> GetById(Guid id, CancellationToken cancellationToken = default)
            => context.VehicleListings.AsNoTracking()
                .Where(l => l.Id == id)
                .Select(Projection)
                .FirstOrDefaultAsync(cancellationToken);

        private static readonly System.Linq.Expressions.Expression<Func<VehicleListing, VehicleListingSummaryDTO>> Projection =
            l => new VehicleListingSummaryDTO(
                l.Id,
                l.DealerId,
                l.DealerCompanyId,
                l.Details.Vin,
                l.Model.Make.Name,
                l.Model.Name,
                l.Details.Year,
                l.Details.MileageKm,
                l.Details.Price.Amount,
                l.Details.Price.Currency,
                l.Status,
                l.CreatedAt,
                l.UpdatedAt);
    }
}
