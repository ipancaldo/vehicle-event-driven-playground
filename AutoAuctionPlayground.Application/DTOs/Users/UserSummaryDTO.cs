namespace AutoAuctionPlayground.Application.DTOs.Users
{
    public sealed record UserSummaryDTO(
        Guid Id,
        string Name,
        Guid CompanyId,
        string CompanyName);
}
