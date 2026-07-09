using MassTransit;

namespace AutoPulse.Infrastructure.Messaging.Sagas
{
    public class AuctionBookingSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; } = string.Empty;

        // Context data 
        public Guid AuctionId { get; set; }
        public Guid WinnerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime? PaymentProcessedAt { get; set; }
    }
}
