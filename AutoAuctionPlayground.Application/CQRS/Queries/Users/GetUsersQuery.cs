using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.DTOs.Users;
using AutoAuctionPlayground.Application.Interfaces.Queries;

namespace AutoAuctionPlayground.Application.CQRS.Queries.Users
{
    public sealed record GetUsersQuery : IQuery<IReadOnlyList<UserSummaryDTO>>;

    public sealed class GetUsersQueryHandler(IUserQueries queries)
        : IQueryHandler<GetUsersQuery, IReadOnlyList<UserSummaryDTO>>
    {
        public Task<IReadOnlyList<UserSummaryDTO>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
            => queries.List(cancellationToken);
    }
}
