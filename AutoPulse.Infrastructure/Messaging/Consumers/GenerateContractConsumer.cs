using AutoPulse.Application.Application.Auctions.Commands.GenerateContract;
using MassTransit;
using MediatR;

namespace AutoPulse.Infrastructure.Messaging.Consumers
{
    public class GenerateContractConsumer : IConsumer<GenerateContractCommand>
    {
        private readonly IMediator _mediator;

        public GenerateContractConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<GenerateContractCommand> context)
        {
            await _mediator.Send(context.Message);
        }
    }
}
