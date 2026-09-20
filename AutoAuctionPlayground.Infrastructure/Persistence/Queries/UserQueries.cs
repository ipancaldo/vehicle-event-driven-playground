using AutoAuctionPlayground.Application.DTOs.Users;
using AutoAuctionPlayground.Application.Interfaces.Queries;
using Microsoft.EntityFrameworkCore;

namespace AutoAuctionPlayground.Infrastructure.Persistence.Queries
{
    public class UserQueries(AutoAuctionDbContext context) : IUserQueries
    {
        public async Task<IReadOnlyList<UserSummaryDTO>> List(CancellationToken cancellationToken = default)
            => await context.Users.AsNoTracking()
                .OrderBy(user => user.Company.Name).ThenBy(user => user.Name)
                .Select(user => new UserSummaryDTO(
                    user.Id,
                    user.Name,
                    user.CompanyId,
                    user.Company.Name))
                .ToListAsync(cancellationToken);
    }
}
