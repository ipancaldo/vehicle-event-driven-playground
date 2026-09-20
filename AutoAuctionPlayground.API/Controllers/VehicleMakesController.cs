using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.CQRS.Queries.VehicleMakes;
using AutoAuctionPlayground.Application.DTOs.VehicleMakes;
using Microsoft.AspNetCore.Mvc;

namespace AutoAuctionPlayground.API.Controllers
{
    [ApiController]
    [Route("api/vehicle-makes")]
    public class VehicleMakesController(IQueryDispatcher queryDispatcher) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var makes = await queryDispatcher.Dispatch<GetVehicleMakesQuery, IReadOnlyList<VehicleMakeDTO>>(
                new GetVehicleMakesQuery(), cancellationToken);

            return Ok(makes);
        }
    }
}
