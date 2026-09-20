using AutoAuctionPlayground.Application.DTOs.VehicleMakes;

namespace AutoAuctionPlayground.Application.Interfaces.Queries
{
    public interface IVehicleMakeQueries
    {
        // Only active makes/models: the domain rejects inactive ones at listing creation, so the
        // catalogue must not offer combinations that would just fail.
        Task<IReadOnlyList<VehicleMakeDTO>> ListActive(CancellationToken cancellationToken = default);
    }
}
