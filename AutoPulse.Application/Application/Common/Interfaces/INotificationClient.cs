namespace AutoPulse.Application.Application.Common.Interfaces
{
    public interface INotificationClient
    {
        Task SendAuctionCreatedNotificationAsync(
            Guid userId,
            Guid auctionId,
            string priority,
            string channel,
            string target,
            string templateId,
            Dictionary<string, string> templateVariables,
            CancellationToken cancellationToken = default
        );
    }
}
