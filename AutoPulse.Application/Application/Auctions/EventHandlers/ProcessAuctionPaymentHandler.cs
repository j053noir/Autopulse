using AutoPulse.Application.Application.Common.Events;
using AutoPulse.Application.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Application.Auctions.EventHandlers
{
    internal class ProcessAuctionPaymentHandler : INotificationHandler<AuctionEndedEvent>
    {
        private readonly ILogger<ProcessAuctionPaymentHandler> _logger;
        private readonly IPaymentService _paymentService;

        public ProcessAuctionPaymentHandler
        (
            ILogger<ProcessAuctionPaymentHandler> logger,
            IPaymentService paymentService
        )
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        public async Task Handle(AuctionEndedEvent notification, CancellationToken cancellationToken)
        {
            var paymentRequest = new PaymentRequest(
                Guid.NewGuid(),
                notification.WinnerId,
                notification.FinalPrice,
                notification.Currency,
                "CreditCard" // This could be dynamic based on user preference
            );
            await _paymentService.ProcessPaymentAsync(paymentRequest, cancellationToken);
        }
    }
}
