namespace AutoPulse.Application.Application.Common.Interfaces
{
    public record PaymentRequest(
        Guid TransactionId,
        Guid UserId,
        decimal Amount,
        string Currency,
        string PaymentMethod
    );

    public record PaymentResponse(
        bool IsSuccess,
        string TransactionReference,
        string? ErrorMessage,
        DateTime ProcessedAt
    );

    public interface IPaymentService
    {
        Task<PaymentResponse> GetPaymentResponseAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default);
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default);
    }
}
