using AutoPulse.Application.Application.Auctions.Commands.CreateAuction;
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

        [HttpPost]
        public async Task<IDictionary<string, Guid>> Create([FromBody] CreateAuctionCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return new Dictionary<string, Guid> { { "id", result } };
        }
    }
}
