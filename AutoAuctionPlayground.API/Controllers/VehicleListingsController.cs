using AutoAuctionPlayground.Application.CQRS.Abstractions;
using AutoAuctionPlayground.Application.CQRS.Commands.VehicleListings;
using AutoAuctionPlayground.Application.CQRS.Queries.VehicleListings;
using AutoAuctionPlayground.Application.DTOs.VehicleListings;
using AutoAuctionPlayground.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AutoAuctionPlayground.API.Controllers
{
    [ApiController]
    [Route("api/vehicle-listings")]
    public class VehicleListingsController(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] VehicleListingStatus? status,
            [FromQuery] Guid? dealerCompanyId,
            CancellationToken cancellationToken)
        {
            var listings = await queryDispatcher.Dispatch<GetVehicleListingsQuery, IReadOnlyList<VehicleListingSummaryDTO>>(
                new GetVehicleListingsQuery(status, dealerCompanyId), cancellationToken);

            return Ok(listings);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var listing = await queryDispatcher.Dispatch<GetVehicleListingQuery, VehicleListingSummaryDTO?>(
                new GetVehicleListingQuery(id), cancellationToken);

            return listing is null ? NotFound() : Ok(listing);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleListingRequestDTO dto, CancellationToken cancellationToken)
        {
            var id = await commandDispatcher.Dispatch<CreateVehicleListingCommand, Guid>(
                new CreateVehicleListingCommand(dto), cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleListingRequestDTO dto, CancellationToken cancellationToken)
        {
            await commandDispatcher.Dispatch(new UpdateVehicleListingCommand(id, dto), cancellationToken);

            return NoContent();
        }

        [HttpPost("{id:guid}/publish")]
        public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
        {
            await commandDispatcher.Dispatch(new PublishVehicleListingCommand(id), cancellationToken);

            return NoContent();
        }

        [HttpPost("{id:guid}/sell")]
        public async Task<IActionResult> Sell(Guid id, [FromBody] SellVehicleListingDTO dto, CancellationToken cancellationToken)
        {
            await commandDispatcher.Dispatch(new SellVehicleListingCommand(id, dto), cancellationToken);

            return NoContent();
        }
    }
}
