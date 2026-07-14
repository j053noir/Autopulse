using AutoPulse.Notifications.Shared.Contracts;
using AutoPulse.Notifications.Workers.Interfaces;
using AutoPulse.Notifications.Workers.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoPulse.Notifications.Workers.Services
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly ILogger<NotificationDispatcher> _logger;
        private readonly ISmsProvider _smsProvider;
        private readonly IEmailProvider _emailProvider;
        private readonly IPushProvider _pushProvider;

        public NotificationDispatcher
        (
            ILogger<NotificationDispatcher> logger,
            ISmsProvider smsProvider,
            IEmailProvider emailProvider,
            IPushProvider pushProvider
        )
        {
            _smsProvider = smsProvider;
            _emailProvider = emailProvider;
            _pushProvider = pushProvider;
            _logger = logger;
        }

        public async Task DispatchAsync(NotificationEventPayload notificationPayload, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Processing channel {notificationPayload.Channel} for user {notificationPayload.UserId}");

            var renderedTitle = GetTitle(notificationPayload.TemplateId, notificationPayload.TemplateVariables);
            var renderedMessage = RenderTemplate(notificationPayload.TemplateId, notificationPayload.TemplateVariables);

            switch (notificationPayload.Channel.ToLowerInvariant())
            {
                case "sms":
                    await _smsProvider.SendSmsAsync(notificationPayload.Target, renderedMessage, cancellationToken);
                    break;

                case "email":
                    await _emailProvider.SendEmailAsync(notificationPayload.Target, renderedTitle, renderedMessage, cancellationToken);
                    break;

                case "push":
                    await _pushProvider.SendAsync(notificationPayload.Target, renderedTitle, renderedMessage, cancellationToken);
                    break;

                default:
                    _logger.LogWarning($"Channel {notificationPayload.Channel} not supported");
                    break;
            }
        }

        private static string GetTitle(string templateId, Dictionary<string, string> templateVariables)
        {
            string rawTitle= templateId switch
            {
                "status" => "Auction \"{Auction} \" is {Status}.",
                "auction_won" => "Auction \"{Auction} \" ended, congratulations you won!.",
                "auction_lose" => "Auction \"{Auction} \" ended, sorry somebody else won.",
                "outbid" => "Hurry up! some outbid you in the auction \"{Auction} \".",
                "payment_received" => "Payment received for auction \"{Auction} \".",
                "payment_rejected" => "Payment rejected for auction \"{Auction} \".",
                "marketing" => "Autopulse this week offers",
                "newsletter" => "Autopulse this week news",
                _ => throw new ArgumentException($"templateId {templateId} is not a valid template"),
            };

            foreach (var (key, value) in templateVariables)
            {
                rawTitle = rawTitle.Replace("{" + key + "}", value);
            }

            return rawTitle;
        }

        private static string RenderTemplate(string templateId, Dictionary<string, string> templateVariables)
        {
            // TODO: Implement dynamic template engine
            string rawTemplate = templateId switch
            {
                "status" => "Hello {User}, the auction {Auction} is {Status}.",
                "auction_won" => "Hello {User}, you won the {Auction} by bidding {Price} {Currency}.",
                "auction_lose" => "Hello {User}, you lose the {Auction} someone else outbid you, by offering {Price} {Currency}.",
                "outbid" => "Hello {User}, someone outbid you in the {Auction} by offering {Price} {Currency}.",
                "payment_received" => "Hello {User}, the payment for {Auction} was received.",
                "payment_rejected" => "Hello {User}, the payment for {Auction} was rejected.",
                "marketting" => "Hello {User}, these are this week offers: \n{Offers}.",
                "newsletter" => "Hello {User}, these are this week news: \n{News}.",
                _ => throw new ArgumentException($"templateId {templateId} is not a valid template"),
            };

            foreach (var (key, value) in templateVariables)
            {
                rawTemplate = rawTemplate.Replace("{" + key + "}", value);
            }

            return rawTemplate;
        }
    }
}
