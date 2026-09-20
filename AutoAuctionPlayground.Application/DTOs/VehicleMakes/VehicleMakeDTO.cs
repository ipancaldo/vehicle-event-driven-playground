namespace AutoAuctionPlayground.Application.DTOs.VehicleMakes
{
    public sealed record VehicleMakeDTO(
        Guid Id,
        string Name,
        IReadOnlyList<VehicleModelDTO> Models);

    public sealed record VehicleModelDTO(
        Guid Id,
        string Name,
        int ProductionStartYear,
        int? ProductionEndYear);
}
