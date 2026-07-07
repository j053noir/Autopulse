using AutoPulse.Domain.Common;

namespace AutoPulse.Domain.Entities
{
    public class ProcessedTransaction : BaseEntity, IAggregateRoot
    {
        public bool? IsSuccess { get; private set; }
        public string? TransactionReference { get; private set; }
        public string? ErrorMessage { get; private set; }
        public DateTimeOffset? ProcessedAt { get; private set; }

        private ProcessedTransaction() { }

        public static ProcessedTransaction Create
        (
            Guid id,
            string? transactionReference,
            bool? isSuccess = null,
            string? errorMessage = null,
            DateTime? processedAt = null
        )
        {
            return new ProcessedTransaction
            {
                Id = id,
                TransactionReference = transactionReference,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                ProcessedAt = processedAt
            };
        }

        public void Complete(string transactionReference, bool isSuccess, string? errorMessage = null)
        {
            if (ProcessedAt is not null) throw new InvalidOperationException("This transaction has already been completed");

            TransactionReference = transactionReference;
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
