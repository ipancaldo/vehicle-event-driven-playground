using AutoAuctionPlayground.Domain.Entities.Vehicle;

namespace AutoAuctionPlayground.Application.Interfaces.Repositories
{
    public interface IVehicleMakeRepository : IBaseRepository<VehicleMake>
    {
        Task<VehicleMake?> GetWithModelsById(Guid id, CancellationToken cancellationToken = default);
        Task<VehicleModel?> GetModelById(Guid modelId, CancellationToken cancellationToken = default);
    }
}
