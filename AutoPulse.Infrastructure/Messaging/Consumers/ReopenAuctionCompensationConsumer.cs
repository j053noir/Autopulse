using AutoPulse.Application.Application.Auctions.Commands.GenerateContract;
using AutoPulse.Application.Application.Auctions.Commands.ReopenAuctionCompensation;
using MassTransit;
using MediatR;

namespace AutoPulse.Infrastructure.Messaging.Consumers
{
    public class ReopenAuctionCompensationConsumer : IConsumer<ReopenAuctionCompensationCommand>
    {
        private readonly IMediator _mediator;

        public ReopenAuctionCompensationConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<ReopenAuctionCompensationCommand> context)
        {
            await _mediator.Send(context.Message);
        }
    }
}
