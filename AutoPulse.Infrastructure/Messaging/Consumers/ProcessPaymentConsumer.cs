using AutoPulse.Application.Application.Auctions.Commands.ProcessPayment;
using MassTransit;
using MediatR;

namespace AutoPulse.Infrastructure.Messaging.Consumers
{
    public class ProcessPaymentConsumer : IConsumer<ProcessPaymentCommand>
    {
        private readonly IMediator _mediator;

        public ProcessPaymentConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
        {
            await _mediator.Send(context.Message);
        }
    }
}
