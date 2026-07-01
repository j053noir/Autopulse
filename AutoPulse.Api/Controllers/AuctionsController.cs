using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
using AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid;
using AutoPulse.Application.Application.Auctions.Queries.ActiveAuctionsWithVehicle;
using AutoPulse.Application.Application.Auctions.Queries.GetAuctionById;
using AutoPulse.Domain.Common.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPulse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : BaseController
    {
        private readonly IMediator _mediator;

        public AuctionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAuctions([FromQuery] GetAuctionWithVehicleQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var query = new GetAuctionByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    Message = $"Auction with ID {id} was not found"
                });
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Auctions.Create)]
        public async Task<IActionResult> Create([FromBody] CreateAuctionCommand command, CancellationToken cancellationToken)
        {
            var auctioneerId = GetUserClaimId(nameof(CreateAuctionBidCommand.AuctioneerId));
            var updatedCommand = command with { AuctioneerId = auctioneerId };

            var result = await _mediator.Send(updatedCommand, cancellationToken);

            return Ok(new Dictionary<string, Guid> { { "id", result } }); ;
        }

        [HttpPost("{id:guid}/bids")]
        [Authorize(Policy = Permissions.Auctions.Bid)]
        public async Task<IActionResult> Create(
            [FromRoute] Guid id,
            [FromBody] CreateAuctionBidCommand command, CancellationToken cancellationToken)
        {
            var bidderId = GetUserClaimId(nameof(CreateAuctionBidCommand.AuctioneerId));

            // Update auction id to use the one from the route parameter instead of the one from the request body
            // Update auctioneer Id to use the one from the token claim
            var updatedCommand = command with { AuctionId = id, AuctioneerId = bidderId};

            var result = await _mediator.Send(updatedCommand, cancellationToken);

            return Ok(new Dictionary<string, Guid> { { "id", result } }); ;
        }
    }
}
