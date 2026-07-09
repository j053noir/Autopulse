using AutoPulse.Application.Application.Common.Events;
using AutoPulse.Application.Application.Common.Messaging.Events;
using AutoPulse.Application.Application.Auctions.Commands.ProcessPayment;
using AutoPulse.Application.Application.Auctions.Commands.GenerateContract;
using AutoPulse.Application.Application.Auctions.Commands.ReopenAuctionCompensation;
using MassTransit;

namespace AutoPulse.Infrastructure.Messaging.Sagas
{
    public class AuctionBookingSaga : MassTransitStateMachine<AuctionBookingSagaState>
    {
        public Event<AuctionEndedIntegrationEvent> AuctionEnded { get; private set; } = null!;
        public Event<PaymentSucceededEvent> PaymentSucceeded { get; private set; } = null!;
        public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = null!;
        public Event<AuctionReopenedEvent> AuctionReopened { get; private set; } = null!;

        public State ProcessingPayment { get; private set; } = null!;
        public State Completed { get; private set; } = null!;
        public State Compensating { get; private set; } = null!;

        public AuctionBookingSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => AuctionEnded, x => x.CorrelateById(context => context.Message.EventId));
            Event(() => PaymentSucceeded, x => x.CorrelateById(context => context.Message.EventId));
            Event(() => PaymentFailed, x => x.CorrelateById(context => context.Message.EventId));
            Event(() => AuctionReopened, x => x.CorrelateById(context => context.Message.EventId));

            During(Initial,
                When(AuctionEnded)
                .Then(context =>
                {
                    context.Saga.AuctionId = context.Message.AuctionId;
                    context.Saga.WinnerId = context.Message.WinnerId;
                    context.Saga.TotalAmount = context.Message.Amount;
                    context.Saga.Currency = context.Message.Currency;
                })
                .TransitionTo(ProcessingPayment)
                .Send(new Uri("queue:process-payment-service"), context => new ProcessPaymentCommand(
                    context.Saga.CorrelationId,
                    context.Saga.AuctionId,
                    context.Saga.WinnerId,
                    context.Saga.TotalAmount,
                    context.Saga.Currency
                ))
            );

            During(ProcessingPayment,
                When(PaymentSucceeded)
                    .Then(context => context.Saga.PaymentProcessedAt = context.Message.OccuredOn)
                    .TransitionTo(Completed)
                    .Send(new Uri("queue:contract-service"), context => new GenerateContractCommand(
                        context.Saga.AuctionId, 
                        context.Saga.WinnerId
                    )),
                When(PaymentFailed)
                    .TransitionTo(Compensating)
                    .Send(new Uri("queue:auction-control-service"), context => new ReopenAuctionCompensationCommand(
                        context.Saga.CorrelationId,
                        context.Saga.AuctionId, 
                        context.Message.ErrorMessage
                    ))
            );

            During(Compensating,
                When(AuctionReopened)
                    .Then(context => LogRecovery(context.Saga.CorrelationId))
                    .Finalize()
            );
        }

        private void LogRecovery(Guid correlationId)
        {
            // TODO: Telemetry logic
            // TOOD: Log successful compensation
        }
    }
}
