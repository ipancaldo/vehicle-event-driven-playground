using AutoAuctionPlayground.Application.DTOs.VehicleMakes;
using AutoAuctionPlayground.Application.Interfaces.Queries;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Queries
{
    public class VehicleMakeQueries(AutoAuctionDbContext context) : IVehicleMakeQueries
    {
        public async Task<IReadOnlyList<VehicleMakeDTO>> ListActive(CancellationToken cancellationToken = default)
            => await context.VehicleMakes.AsNoTracking()
                .Where(make => make.IsActive)
                .OrderBy(make => make.Name)
                .Select(make => new VehicleMakeDTO(
                    make.Id,
                    make.Name,
                    make.Models
                        .Where(model => model.IsActive)
                        .OrderBy(model => model.Name)
                        .Select(model => new VehicleModelDTO(
                            model.Id,
                            model.Name,
                            model.ProductionStartYear,
                            model.ProductionEndYear))
                        .ToList()))
                .ToListAsync(cancellationToken);
    }
}
