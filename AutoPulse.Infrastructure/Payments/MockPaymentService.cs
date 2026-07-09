using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Application.Application.Common.Messaging.Events;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Common.Specification;
using AutoPulse.Domain.Entities;
using AutoPulse.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimiting;
using Polly.Registry;

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
        private readonly ResiliencePipeline _resiliencePipeline;
        private readonly IEventBus _eventBus;

        public MockPaymentService
        (
            ICacheService cacheService,
            ILogger<MockPaymentService> logger,
            IServiceProvider serviceProvider,
            ResiliencePipelineProvider<string> pipelineProvider,
            IEventBus eventBus
        )
        {
            _cacheService = cacheService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _resiliencePipeline = pipelineProvider.GetPipeline(ResilienceConfiguration.GatewayPaymentPolicy);
            _eventBus = eventBus;
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
                        TransactionReference: processedTransaction.TransactionReference!,
                        ErrorMessage: processedTransaction.ErrorMessage,
                        ProcessedAt: processedTransaction.ProcessedAt.Value.DateTime
                    );

                    await AddTransactionToCacheAsync(paymentRequest, processedReponse, cancellationToken);

                    return processedReponse;
                }

                return null;
            }
            catch (Exception dbEx)
            {
                _logger.LogCritical(dbEx, "Catastrophic failure: Relational database query failed for TransactionId: {TransactionId}", paymentRequest.TransactionId);
                return null;
            }
        }

        private async Task AddTransactionToCacheAsync(PaymentRequest paymentRequest, PaymentResponse paymentResponse, CancellationToken cancellationToken = default)
        {
            try
            {
                var idempotencyKey = GetIdemptoncyKey(paymentRequest);
                
                await _cacheService.SetAsync(idempotencyKey, paymentResponse, TimeSpan.FromHours(24), cancellationToken); // Ajustado a 24h para resiliencia financiera
                
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
            var transactionEntity = await StartProcessingPaymentAsync(paymentRequest, scope, cancellationToken);

            PaymentResponse paymentResponse;

            try
            {
                paymentResponse = await _resiliencePipeline.ExecuteAsync(async token =>
                    await SendRequestToPaymentGatewayAsync(paymentRequest, token), cancellationToken);
            }
            catch (RateLimiterRejectedException rateEx)
            {
                _logger.LogWarning(rateEx, "Concurrency limit reached! Rolling back transaction entry for TransactionId: {TransactionId} to retry later", paymentRequest.TransactionId);

                var processedTransactionRepository = scope.ServiceProvider.GetRequiredService<IRepository<ProcessedTransaction>>();
                processedTransactionRepository.Delete(transactionEntity);

                var dbContext = scope.ServiceProvider.GetRequiredService<IAutoPulseDbContext>();
                await dbContext.SaveChangesAsync(cancellationToken);

                throw; // Relanzamos correctamente para que el message broker se entere y reintente
            }
            catch (BrokenCircuitException bcEx)
            {
                _logger.LogWarning("Payment Gateway circuit is OPEN. Fast-failing transaction {TransactionId}. Reason: {Message}", paymentRequest.TransactionId, bcEx.Message);

                paymentResponse = new PaymentResponse(
                    IsSuccess: false,
                    TransactionReference: $"FAIL_CIRCUIT_{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    ErrorMessage: "Payment service is temporarily unavailable due to high failure rates.",
                    ProcessedAt: DateTime.UtcNow
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resilience pipeline intercepted a failure or timeout for TransactionId: {TransactionId}", paymentRequest.TransactionId);

                paymentResponse = new PaymentResponse(
                    IsSuccess: false,
                    TransactionReference: $"FAIL_{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    ErrorMessage: $"Polly Resilient Interception: {ex.Message}",
                    ProcessedAt: DateTime.UtcNow
                );
            }

            await SavePaymentResponse(paymentRequest, paymentResponse, transactionEntity, scope, cancellationToken);

            return paymentResponse;
        }

        private async Task<ProcessedTransaction> StartProcessingPaymentAsync(PaymentRequest paymentRequest, IServiceScope scope, CancellationToken cancellationToken = default)
        {
            var processedTransaction = ProcessedTransaction.Create(paymentRequest.TransactionId, paymentRequest.TransactionId.ToString());

            try
            {
                var processedTransactionRepository = scope.ServiceProvider.GetRequiredService<IRepository<ProcessedTransaction>>();
                processedTransactionRepository.Add(processedTransaction);

                var dbContext = scope.ServiceProvider.GetRequiredService<IAutoPulseDbContext>();
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Processed payment started for TransactionId: {TransactionId}, TransactionReference: {TransactionReference}", paymentRequest.TransactionId, processedTransaction.TransactionReference);

                return processedTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing payment for TransactionId: {TransactionId}", paymentRequest.TransactionId);
                throw;
            }
        }

        private async Task<PaymentResponse> SendRequestToPaymentGatewayAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken); // Reducido a 2s para desarrollo dinámico

            var isSuccess = paymentRequest.Amount > 0;
            var transactionReference = Guid.NewGuid().ToString();
            var errorMessage = isSuccess ? null : "Payment processing failed.";

            return new PaymentResponse(isSuccess, transactionReference, errorMessage, DateTime.UtcNow);
        }

        private async Task SavePaymentResponse
        (
            PaymentRequest paymentRequest,
            PaymentResponse paymentResponse,
            ProcessedTransaction processedTransaction,
            IServiceScope scope,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // 1. Update the processed transaction with the payment response details
                processedTransaction.Complete(paymentResponse.TransactionReference, paymentResponse.IsSuccess, paymentResponse.ErrorMessage);

                // 2. Save changes to the database
                var dbContext = scope.ServiceProvider.GetRequiredService<IAutoPulseDbContext>();
                await dbContext.SaveChangesAsync(cancellationToken);

                // 3. Send Event to Message Bus
                if (paymentResponse.IsSuccess)
                {
                    var succeededEvent = new PaymentSucceededEvent
                    (
                        EventId: Guid.NewGuid(),
                        TransactionId: paymentRequest.TransactionId,
                        TransactionReference: paymentResponse.TransactionReference,
                        AmountProcessed: paymentRequest.Amount,
                        Currency: paymentRequest.Currency,
                        PaymentMethod: paymentRequest.PaymentMethod,
                        OccuredOn: paymentResponse.ProcessedAt
                    );

                    await _eventBus.PublishAsync(succeededEvent, cancellationToken);
                }
                else
                {
                    var failedEvent = new PaymentFailedEvent
                    (
                        EventId: Guid.NewGuid(),
                        TransactionId: paymentRequest.TransactionId,
                        ErrorCode: paymentResponse.ErrorMessage == "Insufficient funds." ? "INSUFFICIENT_FUNDS" : "GATEWAY_ERROR",
                        ErrorMessage: paymentResponse.ErrorMessage ?? "Unknown payment error.",
                        OccuredOn: DateTime.UtcNow
                    );

                    await _eventBus.PublishAsync(failedEvent, cancellationToken);
                }

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