using MediatR;

namespace AutoPulse.Application.Application.Auctions.Commands.ProcessPayment
{
    public record ProcessPaymentCommand(
        Guid CorrelationId,
        Guid TransactionId,
        Guid WinnerId,
        decimal TotalAmount,
        string Currency
    ) : IRequest<bool>;
}
