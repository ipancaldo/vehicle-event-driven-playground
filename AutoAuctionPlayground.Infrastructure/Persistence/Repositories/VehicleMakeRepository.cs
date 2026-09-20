using AutoAuctionPlayground.Application.Interfaces.Repositories;
using AutoAuctionPlayground.Domain.Entities.Vehicle;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Repositories
{
    public class VehicleMakeRepository(AutoAuctionDbContext context)
        : BaseRepository<VehicleMake>(context), IVehicleMakeRepository
    {
        public Task<VehicleMake?> GetWithModelsById(Guid id, CancellationToken cancellationToken = default)
            => _dbSet.Include(m => m.Models).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        public Task<VehicleModel?> GetModelById(Guid modelId, CancellationToken cancellationToken = default)
            => _context.VehicleModels.Include(m => m.Make).FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);
    }
}
