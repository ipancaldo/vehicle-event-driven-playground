using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.CQRS.Queries.VehicleListings;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AutoAuctionPlayground.API.Controllers
{
    [ApiController]
    [Route("api/vehicle-listings")]
    public class VehicleListingsController(IQueryDispatcher queryDispatcher) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] VehicleListingStatus? status,
            [FromQuery] Guid? dealerCompanyId,
            CancellationToken cancellationToken)
        {
            var listings = await queryDispatcher.Dispatch<GetVehicleListingsQuery, IReadOnlyList<VehicleListingSummaryDto>>(
                new GetVehicleListingsQuery(status, dealerCompanyId), cancellationToken);

            return Ok(listings);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var listing = await queryDispatcher.Dispatch<GetVehicleListingQuery, VehicleListingSummaryDto?>(
                new GetVehicleListingQuery(id), cancellationToken);

            return listing is null ? NotFound() : Ok(listing);
        }
    }
}
