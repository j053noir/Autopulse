using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
using AutoPulse.Application.Application.Auctions.Commands.CreateAuctionBid;
using AutoPulse.Application.Application.Auctions.Queries.ActiveAuctionsWithVehicle;
using AutoPulse.Application.Application.Auctions.Queries.Common.Dto;
using AutoPulse.Application.Application.Auctions.Queries.GetAuctionById;
using AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard;
using AutoPulse.Application.Application.Auctions.Queries.GetAuctionDashboard.Dto;
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

        /// <summary>
        /// Retrieves a list of all active auctions along with their vehicle details.
        /// </summary>
        /// <param name="query">The query parameters to filter active auctions.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of active auctions with their associated vehicle details.</returns>
        /// <response code="200">Returns the list of active auctions.</response>
        /// <response code="401">If the request is unauthorized.</response>
        /// <response code="403">If the user does not have permission to read auctions.</response>
        [HttpGet("active")]
        [Authorize(Policy = Permissions.Auctions.Read)]
        [ProducesResponseType(typeof(IReadOnlyList<AuctionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetActiveAuctions([FromQuery] GetAuctionWithVehicleQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the details of a specific auction by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the auction.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The details of the requested auction.</returns>
        /// <response code="200">Returns the requested auction details.</response>
        /// <response code="401">If the request is unauthorized.</response>
        /// <response code="403">If the user does not have permission to read auctions.</response>
        /// <response code="404">If the auction with the specified ID is not found.</response>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = Permissions.Auctions.Read)]
        [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Retrieves the dashboard details for a specific auction, including bids history.
        /// </summary>
        /// <param name="id">The unique identifier of the auction.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The auction dashboard details.</returns>
        /// <response code="200">Returns the auction dashboard details.</response>
        /// <response code="401">If the request is unauthorized.</response>
        /// <response code="403">If the user does not have permission to read auctions.</response>
        /// <response code="404">If the auction was not found.</response>
        [HttpGet("{id:guid}/dashboard")]
        [Authorize(Policy = Permissions.Auctions.Read)]
        [ProducesResponseType(typeof(AuctionDashboardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDashboard([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var query = new GetAuctionDashboardQuery(id);
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

        /// <summary>
        /// Creates a new auction.
        /// </summary>
        /// <param name="command">The details of the auction to create.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The unique identifier of the newly created auction.</returns>
        /// <response code="200">Returns the ID of the created auction.</response>
        /// <response code="400">If the request payload is invalid.</response>
        /// <response code="401">If the request is unauthorized.</response>
        /// <response code="403">If the user does not have permission to create auctions.</response>
        [HttpPost]
        [Authorize(Policy = Permissions.Auctions.Create)]
        [ProducesResponseType(typeof(Dictionary<string, Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateAuctionCommand command, CancellationToken cancellationToken)
        {
            var auctioneerId = GetUserClaimId(nameof(CreateAuctionCommand.AuctioneerId));
            var updatedCommand = command with { AuctioneerId = auctioneerId };

            var result = await _mediator.Send(updatedCommand, cancellationToken);

            return Ok(new Dictionary<string, Guid> { { "id", result } }); ;
        }

        /// <summary>
        /// Places a new bid on a specific auction.
        /// </summary>
        /// <param name="id">The unique identifier of the auction to place a bid on.</param>
        /// <param name="command">The details of the bid to place.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The unique identifier of the newly created bid.</returns>
        /// <response code="200">Returns the ID of the created bid.</response>
        /// <response code="400">If the bid is invalid or lower than required.</response>
        /// <response code="401">If the request is unauthorized.</response>
        /// <response code="403">If the user does not have permission to place bids.</response>
        [HttpPost("{id:guid}/bids")]
        [Authorize(Policy = Permissions.Auctions.Bid)]
        [ProducesResponseType(typeof(Dictionary<string, Guid>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromRoute] Guid id,
            [FromBody] CreateAuctionBidCommand command, CancellationToken cancellationToken)
        {
            var bidderId = GetUserClaimId(nameof(CreateAuctionBidCommand.BidderId));

            // Update auction id to use the one from the route parameter instead of the one from the request body
            // Update auctioneer Id to use the one from the token claim
            var updatedCommand = command with { AuctionId = id, BidderId = bidderId};

            var result = await _mediator.Send(updatedCommand, cancellationToken);

            return Ok(new Dictionary<string, Guid> { { "id", result } }); ;
        }
    }
}
