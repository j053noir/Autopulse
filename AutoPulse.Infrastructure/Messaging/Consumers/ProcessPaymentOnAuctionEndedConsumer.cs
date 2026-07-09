//using AutoPulse.Application.Application.Common.Events;
//using AutoPulse.Application.Application.Common.Interfaces;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Text;

namespace AutoPulse.Infrastructure.Messaging.Consumers
{
    //public class ProcessPaymentOnAuctionEndedConsumer
    //{
    //    private readonly IPaymentService _paymentService;
    //    private readonly ILogger<ProcessPaymentOnAuctionEndedConsumer> _logger;

    //    public ProcessPaymentOnAuctionEndedConsumer(
    //        IPaymentService paymentService,
    //        ILogger<ProcessPaymentOnAuctionEndedConsumer> logger
    //    )
    //    {
    //        _paymentService = paymentService;
    //        _logger = logger;
    //    }

    //    public async Task ConsumeAsync(AuctionEndedIntegrationEvent message, CancellationToken cancellationToken = default)
    //    {
    //        _logger.LogInformation("Message received from Broker: Auction {AuctionId} ended. Initializing payment workflow...", message.AuctionId);

    //        var paymentRequest = new PaymentRequest(
    //            TransactionId: message.EventId,
    //            UserId: message.WinnerId,
    //            Amount: message.Amount,
    //            Currency: message.Currency,
    //            PaymentMethod: "tok_visa_mocked"
    //        );

    //        var paymentResponse = await _paymentService.GetPaymentResponseAsync(paymentRequest, cancellationToken);

    //        if (paymentResponse.IsSuccess)
    //        {
    //            _logger.LogInformation("Payment processed successfully for Auction {AuctionId}. Transaction Reference: {TransactionReference}", message.AuctionId, paymentResponse.TransactionReference);
    //            // TODO: Implement any post-payment success actions, such as notifying the winner or updating auction status
    //        }
    //        else
    //        {
    //            _logger.LogError("Payment processing failed for Auction {AuctionId}. Error: {ErrorMessage}", message.AuctionId, paymentResponse.ErrorMessage);
    //            // TODO: Implement retry logic or compensation actions as needed
    //        }
    //    }
    //}
}
