using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Infrastructure.Payments
{
    internal class ProcessedTransactionSpecification : BaseSpecification<ProcessedTransaction>
    {
        public ProcessedTransactionSpecification(string transactionReference) :
            base(a => a.TransactionReference == transactionReference)
        {
        }
    }

    public class MockPaymentService : IPaymentService
    {
        private readonly ICacheService _cacheService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MockPaymentService> _logger;

        public MockPaymentService
        (
            ICacheService cacheService,
            ILogger<MockPaymentService> logger,
            IServiceProvider serviceProvider
        )
        {
            _cacheService = cacheService;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<PaymentResponse> GetPaymentResponseAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            var idempotencyKey = GetIdemptoncyKey(paymentRequest);

            // 1. Check if the response is already cached
            try
            {
                var cachedResponse = await _cacheService.GetAsync<PaymentResponse>(idempotencyKey, cancellationToken);

                if (cachedResponse is not null)
                {
                    _logger.LogInformation("Returning cached payment response for TransactionId: {TransactionId}", paymentRequest.TransactionId);
                    return cachedResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached payment response for TransactionId: {TransactionId}", paymentRequest.TransactionId);
            }

            // 2. Check if the response is already processed in the database
            using var scope = _serviceProvider.CreateScope();
            var processedTransaction = await CheckDatabaseForProcessedTransactionAsync(paymentRequest, scope, cancellationToken);

            if (processedTransaction is not null) return processedTransaction;

            // 3. Payment processing logic
            return await ProcessPaymentAsync(paymentRequest, scope, cancellationToken);
        }

        private string GetIdemptoncyKey(PaymentRequest paymentRequest)
        {
            return $"payments:idempotency:{paymentRequest.TransactionId}";
        }

        private async Task<PaymentResponse?> CheckDatabaseForProcessedTransactionAsync(PaymentRequest paymentRequest, IServiceScope scope, CancellationToken cancellationToken = default)
        {
            try
            {
                var processedTransactionRepository = scope.ServiceProvider.GetRequiredService<IRepository<ProcessedTransaction>>();

                var spec = new ProcessedTransactionSpecification(paymentRequest.TransactionId.ToString());
                var processedTransaction = await processedTransactionRepository.GetBySpecAsync(spec, cancellationToken);

                if (processedTransaction is not null &&
                    processedTransaction.IsSuccess.HasValue &&
                    processedTransaction.ProcessedAt.HasValue)
                {
                    _logger.LogInformation("Transaction {TransactionId} has already been processed.", paymentRequest.TransactionId);

                    var processedReponse = new PaymentResponse
                    (
                        IsSuccess: processedTransaction.IsSuccess.Value,
                        TransactionReference: processedTransaction.TransactionReference,
                        ErrorMessage: processedTransaction.ErrorMessage,
                        ProcessedAt: processedTransaction.ProcessedAt.Value.DateTime
                    );

                    // Cache the processed transaction for future requests
                    await AddTransactionToCacheAsync(paymentRequest, processedReponse, cancellationToken);

                    return processedReponse;
                }

                return null;
            }
            catch (Exception dbEx)
            {
                _logger.LogCritical(dbEx, "Catastrophic failure: Relational database query failed for TransactionId: {TransactionId}");
                return null;
            }
        }

        private async Task AddTransactionToCacheAsync(PaymentRequest paymentRequest, PaymentResponse paymentResponse, CancellationToken cancellationToken = default)
        {
            try
            {
                var idempotencyKey = GetIdemptoncyKey(paymentRequest);

                await _cacheService.SetAsync(idempotencyKey, paymentResponse, TimeSpan.FromHours(1), cancellationToken);

                _logger.LogInformation("Cached payment response for TransactionId: {TransactionId}", paymentRequest.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching payment response for TransactionId: {TransactionId}", paymentRequest.TransactionId);
            }
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();

            return await this.ProcessPaymentAsync(paymentRequest, scope, cancellationToken);
        }

        private async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest, IServiceScope scope, CancellationToken cancellationToken = default)
        {
            await StartProcessingPaymentAsync(paymentRequest, scope, cancellationToken);

            var paymentResponse = await SendRequestToPaymentGatewayAsync(paymentRequest, cancellationToken);

            await SavePaymentResponse(paymentRequest, paymentResponse, scope, cancellationToken);

            return paymentResponse;
        }

        private async Task StartProcessingPaymentAsync(PaymentRequest paymentRequest, IServiceScope scope, CancellationToken cancellationToken = default)
        {
            var processedTransaction = ProcessedTransaction.Create(Guid.NewGuid(), paymentRequest.TransactionId.ToString());

            try
            {
                // 1. Save the processed transaction to the database
                var processedTransactionRepository = scope.ServiceProvider.GetRequiredService<IRepository<ProcessedTransaction>>();
                processedTransactionRepository.Add(processedTransaction);

                // 2. Save changes to the database
                var dbContext = scope.ServiceProvider.GetRequiredService<IAutoPulseDbContext>();
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Processed payment started for TransactionId: {TransactionId}, TransactionReference: {TransactionReference}", paymentRequest.TransactionId, processedTransaction.TransactionReference);
            }
            catch (Exception ex)
            {
                // Log the error and rethrow the exception to be handled by the calling method
                _logger.LogError(ex, "Error occurred while processing payment for TransactionId: {TransactionId}", paymentRequest.TransactionId);
                throw;
            }
        }

        private async Task<PaymentResponse> SendRequestToPaymentGatewayAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            // Simulate payment processing delay
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            var isSuccess = paymentRequest.Amount > 0; // Simulate a failure for negative or zero amounts
            var transactionReference = Guid.NewGuid().ToString();
            var errorMessage = isSuccess ? null : "Payment processing failed.";
            var processedAt = DateTime.UtcNow;

            // Create the payment response
            var paymentResponse = new PaymentResponse(isSuccess, transactionReference, errorMessage, processedAt);

            return paymentResponse;
        }
        private async Task SavePaymentResponse(PaymentRequest paymentRequest, PaymentResponse paymentResponse, IServiceScope scope, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Retrieve the processed transaction from the database
                var processedTransactionRepository = scope.ServiceProvider.GetRequiredService<IRepository<ProcessedTransaction>>();
                var spec = new ProcessedTransactionSpecification(paymentRequest.TransactionId.ToString());
                var processedTransaction = await processedTransactionRepository.GetBySpecAsync(spec, cancellationToken);

                if (processedTransaction is null) throw new KeyNotFoundException($"Processed transaction with Id {paymentResponse.TransactionReference} not found.");

                // 2. Update the processed transaction with the payment response details
                processedTransaction.Complete(paymentResponse.IsSuccess, paymentResponse.ErrorMessage);

                // 3. Save changes to the database
                var dbContext = scope.ServiceProvider.GetRequiredService<IAutoPulseDbContext>();
                await dbContext.SaveChangesAsync(cancellationToken);

                // 4. Cache the payment response for future requests
                await AddTransactionToCacheAsync(paymentRequest, paymentResponse, cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving payment response for TransactionReference: {TransactionReference}", paymentResponse.TransactionReference);
                throw;
            }
        }
    }
}
