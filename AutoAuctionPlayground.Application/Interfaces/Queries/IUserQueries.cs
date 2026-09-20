using AutoAuctionPlayground.Application.DTOs.Users;

namespace AutoAuctionPlayground.Application.Interfaces.Queries
{
    public interface IUserQueries
    {
        Task<IReadOnlyList<UserSummaryDTO>> List(CancellationToken cancellationToken = default);
    }
}
