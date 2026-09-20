using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.CQRS.Queries.Users;
using AutoAuctionPlayground.Application.DTOs.Users;
using Microsoft.AspNetCore.Mvc;

namespace AutoAuctionPlayground.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController(IQueryDispatcher queryDispatcher) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var users = await queryDispatcher.Dispatch<GetUsersQuery, IReadOnlyList<UserSummaryDTO>>(
                new GetUsersQuery(), cancellationToken);

            return Ok(users);
        }
    }
}
