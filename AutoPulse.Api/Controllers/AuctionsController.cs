using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
using AutoPulse.Application.Application.Auctions.Queries.GetAuctionById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoPulse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : Controller
    {
        private readonly IMediator _mediator;

        public AuctionsController(IMediator mediator)
        {
            _mediator = mediator;
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
        public async Task<IDictionary<string, Guid>> Create([FromBody] CreateAuctionCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return new Dictionary<string, Guid> { { "id", result } };
        }
    }
}
