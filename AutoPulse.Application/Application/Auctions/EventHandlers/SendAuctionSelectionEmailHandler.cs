using AutoPulse.Application.Application.Common.Events;
using AutoPulse.Application.Application.Common.Interfaces;
using AutoPulse.Domain.Common.Interfaces;
using AutoPulse.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Application.Application.Auctions.EventHandlers
{
    public class SendAuctionSelectionEmailHandler : INotificationHandler<AuctionEndedEvent>
    {
        private readonly ILogger<SendAuctionSelectionEmailHandler> _logger;
        private readonly IEmailService _emailService;
        private readonly IRepository<User> _userRepository;

        public SendAuctionSelectionEmailHandler
        (
            ILogger<SendAuctionSelectionEmailHandler> logger,
            IEmailService emailService,
            IRepository<User> userRepository)
        {
            _logger = logger;
            _emailService = emailService;
            _userRepository = userRepository;
        }

        public async Task Handle(AuctionEndedEvent notification, CancellationToken cancellationToken)
        {
            var userEntity = await _userRepository.GetByIdAsync(notification.WinnerId, cancellationToken);

            var userEmail = userEntity?.Email ?? throw new KeyNotFoundException($"User with ID {notification.WinnerId} not found.");

            try
            {
                await _emailService.SendEmailAsync(
                    "do-not-reply@autopulse.com", // from
                    userEmail, // to
                    "Congratulations! You've won the auction!", // subject
                    $"You have won the auction with ID {notification.AuctionId} at a final price of {notification.FinalPrice:C}. Please proceed to complete the payment."
                );

                _logger.LogInformation(
                    "Asynchronous Event Triggered: Sending email notification to winner {WinnerId} for auction {AuctionId}.",
                notification.WinnerId, notification.AuctionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error: Could not send email to winner {WinnerId} for auction {AuctionId}.",
                notification.WinnerId, notification.AuctionId);
            }
        }
    }
}
