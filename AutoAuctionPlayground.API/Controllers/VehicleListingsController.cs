using Microsoft.AspNetCore.Mvc;

namespace AutoAuctionPlayground.API.Controllers
{
    [ApiController]
    [Route("api/vehicle-listings")]
    public class VehicleListingsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(CancellationToken cancellationToken)
        {
            return Ok(Array.Empty<object>());
        }
    }
}
